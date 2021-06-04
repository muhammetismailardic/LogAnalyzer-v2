using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Models
{
    public class RadioConfItems
    {
        public string IpAddress { get; set; }
        public string OppIpAddress { get; set; }
        public string Port { get; set; }
        public string OppPort { get; set; }
        public string SWGroup { get; set; }
        public string OppSWGroup { get; set; }
        public string TXRFFrequency { get; set; }
        public string RXRFFrequency { get; set; }
    }
}
