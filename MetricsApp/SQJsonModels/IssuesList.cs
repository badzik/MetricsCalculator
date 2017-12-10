using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.SQJsonModels
{
    public class IssuesList
    {
        [JsonProperty(PropertyName = "issues")]
        public List<Issue> Issues { get; set; }
    }
}