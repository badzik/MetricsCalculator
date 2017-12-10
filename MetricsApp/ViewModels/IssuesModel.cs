using MetricsApp.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.ViewModels
{
    public class IssuesModel
    {
        public int OpenedIssues { get; set; }
        public int ClosedIssues { get; set; }
        public TimeSpan AverageIssueClosingTime { get; set; }
        public TimeSpan EstimatedTimeToCloseAllIssues { get; set; }
        public List<IssueCountForMonth> ClosedIssuesForMonth { get; set; }
        public DateTime ExpectedDateForClosingAllIssues { get; set; }
        public UserWithCounter UserWithLargestIssuesClosed { get; set; }
    }
}