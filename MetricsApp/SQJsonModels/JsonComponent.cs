using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.SQJsonModels
{
    public class JsonComponent
    {
        [JsonProperty(PropertyName = "component")]
        public Component Component { get; set; }
    }
}