using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Models
{
    public class TimeTable
    {
        public string Ip { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public DateTime Started { get; set; }
        public DateTime Completed { get; set; } = DateTime.MinValue;
        public TimeSpan Duration { get; set; }
        public string PMON { get; set; } = "*";
        public string RMON { get; set; } = "*";
        public string FileCount { get; set; } = "-";
    }
}
