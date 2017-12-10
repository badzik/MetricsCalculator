using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.SQJsonModels
{
    public class Measure
    {
        [JsonProperty(PropertyName = "metric")]
        public string Metric { get; set; }
        [JsonProperty(PropertyName = "value")]
        public Double Value { get; set; }
    }
}