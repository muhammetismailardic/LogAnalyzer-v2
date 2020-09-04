using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Models
{
    public class CollectionSummary
    {
        public List<CollSummData> ColList { get; set; }

        public CollSummData Server
        {
            get
            {
                if (ColList == null) ColList = new List<CollSummData>();
                if (ColList.Count == 0) ColList.Add(new CollSummData());
                return ColList[0];
            }
            set
            {
                if (ColList == null) ColList = new List<CollSummData>();
                if (ColList.Count == 0) ColList.Add(new CollSummData());
                ColList[0] = value;
            }
        }

        public CollectionSummary()
        {
            ColList = new List<CollSummData>();
        }

        public CollectionSummary(int count) : this()
        {
            for (int i = 0; i < count; i++)
            {
                ColList.Add(new CollSummData());
            }
        }
    }

    public class CollSummData
    {
        //public DateTime Date { get; set; }
        //public string Type { get; set; }
        //public string CollectionType { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Completed { get; set; }
        public string Duration { get; set; }
    }
}
