using MetricsApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Models
{
    public class IssueCountWithSeverity
    {
        public int Counter { get; set; }
        public Severity Severity { get; set; }
    }
}