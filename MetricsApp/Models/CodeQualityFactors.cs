using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Models
{
    public class CodeQualityFactors
    {
        public Double CodeQuality { get; set; }
        public Double TestUncoverageRatio { get; set; }
        public Double DuplicatedLinesRatio { get; set; }
        public Double ComplexityRatio { get; set; }
        public Double MajorIssuesRatio { get; set; }
    }
}