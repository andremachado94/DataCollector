using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace DataCollectionBot
{
    public class StoreData : IStoreItem
    {
        public string Gender { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        public string Destiny { get; set; }

        public int Age { get; set; }

        public string ETag { get; set; }

        public List<string> Symptoms { get; set; }
    }
}
