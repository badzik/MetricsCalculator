using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.SQJsonModels
{
    public class Issue
    {
        [JsonProperty(PropertyName = "key")]
        public string key { get; set; }
        [JsonProperty(PropertyName = "rule")]
        public string rule { get; set; }
        [JsonProperty(PropertyName = "severity")]
        public string severity { get; set; }
        [JsonProperty(PropertyName = "component")]
        public string component { get; set; }
        [JsonProperty(PropertyName = "project")]
        public string project { get; set; }
        [JsonProperty(PropertyName = "subProject")]
        public string subProject { get; set; }
        [JsonProperty(PropertyName = "line")]
        public int line { get; set; }
        [JsonProperty(PropertyName = "hash")]
        public string hash { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string status { get; set; }
        [JsonProperty(PropertyName = "effort")]
        public string effort { get; set; }
        [JsonProperty(PropertyName = "debt")]
        public string debt { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string type { get; set; }
    }
}