using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.SQJsonModels
{
    public class IssuesList
    {
        [JsonProperty(PropertyName = "total")]
        public int TotalIssues { get; set; }
        [JsonProperty(PropertyName = "p")]
        public int Page { get; set; }
        [JsonProperty(PropertyName = "ps")]
        public int PageSize { get; set; }
        [JsonProperty(PropertyName = "issues")]
        public List<Issue> Issues { get; set; }
    }
}