// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace DataCollectionBot
{
    public class BotAccessors
    {
        public BotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string DialogStateAccessorName { get; } = $"{nameof(BotAccessors)}.DialogState";

        public static string CurrentQuestionAccessorName { get; } = $"{nameof(BotAccessors)}.CurrentQuestion";

        public static string WelcomeUserAccessorName { get; } = $"{nameof(BotAccessors)}.WelcomeState";

        public static string CounterStateAccessorName { get; } = $"{nameof(BotAccessors)}.CounterState";

        public IStatePropertyAccessor<CounterState> CounterStateStateAccessor { get; internal set; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; internal set; }

        public IStatePropertyAccessor<WelcomeUserState> WelcomeStateAccessor { get; set; }

        public IStatePropertyAccessor<CurrentQuestionState> CurrentQuestionStateAccessor { get; set; }

        public ConversationState ConversationState { get; }
    }
}
