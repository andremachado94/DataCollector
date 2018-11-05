using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataCollectionBot
{
    public class CurrentQuestionState
    {
        public string Question { get; set; }

        public string Destiny { get; set; }

        public string Relation { get; set; }

        public string Gender { get; set; }

        public int Age { get; set; }

        public List<string> Symptoms { get; set; }
    }
}