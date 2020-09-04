using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Models
{
    public class ServiceSession
    {
        public int SessionNo { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; }
        public string StartTime { get; set; }
        public string StopTime { get; set; }
        public string Duration { get; set; }
    }
}
