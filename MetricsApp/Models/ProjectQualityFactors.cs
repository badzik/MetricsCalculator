using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Models
{
    public class ProjectQualityFactors
    {
        public Double ProjectQuality { get; set; }
        public Double CodeQualityRatio { get; set; }
        public CodeQualityFactors CodeQuality { get; set; }
        public Double MinorIssuesRatio { get; set; }
        public Double KnownIssuesRatio { get; set; }
    }
}