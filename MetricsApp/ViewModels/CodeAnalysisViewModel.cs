using MetricsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.ViewModels
{
    public class CodeAnalysisViewModel
    {
        public TimeSpan AverageIssueEffort { get; set; }
        public TimeSpan EstTimeToFixImportantIssues{ get; set; }
        public TimeSpan EstTimeToFixAllIssues { get; set; }
        public DateTime ExpectedDateForClosingAllIssues { get; set; }
        public CodeQualityFactors CodeQuality { get; set; }
        public List<IssueCountWithSeverity> IssuesCountWithSeverity { get; set; }
    }
}