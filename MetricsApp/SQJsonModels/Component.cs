using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.SQJsonModels
{
    public class Component
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "qualifiers")]
        public string Qualifiers { get; set; }
        [JsonProperty(PropertyName = "measures")]
        public List<Measure> Measures { get; set; }
    }
}