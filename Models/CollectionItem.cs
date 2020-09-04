using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Models
{
    public class CollectionItem
    {
        public DateTime Date { get; set; }
        public string Source { get; set; }
        public string Ip { get; set; }
        public string CollectionType { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Completed { get; set; }
        public string Duration { get; set; }
        public string IsTransferCompleted { get; set; }
    }
}
