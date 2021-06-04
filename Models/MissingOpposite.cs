using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Models
{
    public class MissingOpposite
    {
        public string Status { get; set; } = "Issue";
        public string Date { get; set; }
        public string NEType { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string OppIP { get; set; }
        public string OppPort { get; set; } = " ";
        public string OppPortInRadio { get; set; }
        public string OppIpInRmon { get; set; } = " ";
        public string OppPortInRmon { get; set; } = " ";
        public string GroupMember { get; set; } = "-";
        public string OppGroupMember { get; set; } = "-";
        public bool HasRmon { get; set; } = false;
        public bool IsMatch { get; set; } = false;
    }
}
