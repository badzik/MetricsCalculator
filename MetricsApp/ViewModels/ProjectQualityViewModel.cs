using MetricsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.ViewModels
{
    public class ProjectQualityViewModel
    {
        public TimeSpan EstimatedTimeToBetaRelease { get; set; }
        public TimeSpan EstimatedTimeToFullRelease { get; set; }
        public ProjectQualityFactors ProjectQuality { get; set; }
    }
}