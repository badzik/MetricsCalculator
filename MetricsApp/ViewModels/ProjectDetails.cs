using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MetricsApp.ViewModels
{
    public class ProjectDetails
    {
        [Required]
        [Display(Name = "Project name(GitHub)")]
        public string GitHubProjectName { get; set; }
        [Required]
        [Display(Name = "Project owner(GitHub)")]
        public string GitHubProjectOwner { get; set; }
        [Required]
        [Display(Name = "GitHub token")]
        public string GitHubToken { get; set;  }
        [Required]
        [Display(Name = "SonarQube server URL")]
        public string SonarServerUrl { get; set; }
        [Required]
        [Display(Name = "Project name(SonarQube)")]
        public string SonarProjectName { get; set; }
    }
}