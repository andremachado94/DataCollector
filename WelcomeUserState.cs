using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataCollectionBot
{
    public class WelcomeUserState
    {
        public WelcomeUserState(bool b)
        {
            DidBotWelcomeUser = b;
        }

        public WelcomeUserState()
        {
        }

        public bool DidBotWelcomeUser { get; set; } = false;
    }
}
