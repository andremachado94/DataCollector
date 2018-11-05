// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCollectionBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace DataCollectionBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class SimpleBot : IBot
    {
        private const string WelcomeMessage = @"Olá!";

        private readonly BotAccessors _accessors;
        private readonly ILogger _logger;
        private readonly DialogSet dialogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public SimpleBot(BotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<SimpleBot>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            var dialogState = accessors.DialogStateAccessor;

            dialogs = new DialogSet(dialogState);
            dialogs.Add(RootDialog.Instance);
            dialogs.Add(NewSentenceDialog.Instance);
            dialogs.Add(CurrentQuestionDialog.Instance);
            dialogs.Add(new ChoicePrompt("choicePrompt"));
            dialogs.Add(new TextPrompt("textPrompt"));
            dialogs.Add(new NumberPrompt<int>("numberPrompt"));
            BotAccessors = accessors;
        }

        public BotAccessors BotAccessors { get; }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var welcomeUserState = await _accessors.WelcomeStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);
            await _accessors.CurrentQuestionStateAccessor.GetAsync(turnContext, () => new CurrentQuestionState(), cancellationToken);

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (!welcomeUserState.DidBotWelcomeUser)
                {
                    await SetWelcomeStateAsync(true, turnContext, cancellationToken);
                    var userName = turnContext.Activity.From.Name;
                    await turnContext.SendActivityAsync($"Olá {(userName == "User" ? string.Empty : userName)}!\n\nO objetivo deste bot é obter frases de pacientes a explicar os sintomas que têm. O que vamos fazer é o seguinte: Eu vou-te apresentar um sintoma e tu vais simular ser o paciente.\n\nSe eu disser por exemplo:\"És uma mulher de 26 anos\" que teve um sangramento na gravidez tu deverás ser capaz de responder à pergunta \"O que se passa contigo\" interpretando o papel dessa mulher", cancellationToken: cancellationToken);
                }

                await StartRootDialogAsync(turnContext, cancellationToken);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            await SetWelcomeStateAsync(true, turnContext, cancellationToken);
                            await turnContext.SendActivityAsync($"Olá {(member.Name == "User" ? string.Empty : member.Name)}!\n\nO objetivo deste bot é obter frases de pacientes a explicar os sintomas que têm. O que vamos fazer é o seguinte: Eu vou-te apresentar um sintoma e tu vais simular ser o paciente.\n\nSe eu disser por exemplo:\"És uma mulher de 26 anos\" que teve um sangramento na gravidez tu deverás ser capaz de responder à pergunta \"O que se passa contigo\" interpretando o papel dessa mulher", cancellationToken: cancellationToken);
                            await StartRootDialogAsync(turnContext, cancellationToken);
                        }
                    }
                }
            }
            else
            {
                // await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }

        private async Task StartRootDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Get the conversation state from the turn context.
            var state = await _accessors.CounterStateStateAccessor.GetAsync(turnContext, () => new CounterState(), cancellationToken);

            turnContext.TurnState.Add("BotAccessors", BotAccessors);

            var dialogCtx = await dialogs.CreateContextAsync(turnContext, cancellationToken);

            if (dialogCtx.ActiveDialog == null)
            {
                await dialogCtx.BeginDialogAsync(RootDialog.Id, cancellationToken);
            }
            else
            {
                await dialogCtx.ContinueDialogAsync(cancellationToken);
            }

            // Save the new turn count into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext);
        }

        private async Task SetWelcomeStateAsync(bool value, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await _accessors.WelcomeStateAccessor.SetAsync(turnContext, new WelcomeUserState(value), cancellationToken);
            await _accessors.ConversationState.SaveChangesAsync(turnContext);
        }
    }
}
