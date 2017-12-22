using MetricsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Metrics
{
    public class MixedMetrics
    {
        private GitHubMetrics ghClient;
        private SonarQubeMetrics sqClient;

        public MixedMetrics(GitHubMetrics ghClient,SonarQubeMetrics sqClient)
        {
            this.ghClient = ghClient;
            this.sqClient = sqClient;
        }
    
        public async System.Threading.Tasks.Task<ProjectQualityFactors> CalculateQualityFactors()
        {
            CodeQualityFactors cqFactors = sqClient.CalculateCodeQuality();
            ProjectQualityFactors pFactors = new ProjectQualityFactors();
            int openedIssues = await ghClient.CountOpenedIssuesAsync();
            int closedIssues = await ghClient.CountClosedIssuesAsync();
            int allLines = sqClient.GetNumberOfAllLines();
            int numberOfMinorIssues = sqClient.GetNumberOfMinorIssues();

            Double maxNumberOfMinorIssues = Math.Round((double)allLines / 100);
            pFactors.CodeQualityRatio = 0.6;
            if ((openedIssues+closedIssues)>0)
            {
                pFactors.KnownIssuesRatio = Math.Round(((double)openedIssues / (closedIssues + openedIssues))*70.0 * 0.4);
            }
            else
            {
                pFactors.KnownIssuesRatio = Math.Round(70.0 * 0.4);
            }

            pFactors.MinorIssuesRatio = numberOfMinorIssues > maxNumberOfMinorIssues ? Math.Round(30.0 * 0.4): Math.Round(((double)numberOfMinorIssues / maxNumberOfMinorIssues)*30.0*0.4);

            pFactors.ProjectQuality = 100-((Math.Round(pFactors.CodeQualityRatio * (100-cqFactors.CodeQuality))) + pFactors.KnownIssuesRatio + pFactors.MinorIssuesRatio);
            pFactors.CodeQuality = cqFactors;

            return pFactors;
        }

        public async System.Threading.Tasks.Task<TimeSpan> CalculateTimeToBetaRelease()
        {
            TimeSpan timeToBetaRelease;
            int numberOfContributors = await ghClient.GetNumberOfActiveContributorsAsync();
            TimeSpan timeToCloseImportantIssues=sqClient.CalculateTimeForClosingAllImportantIssues();

            TimeSpan timeToCloseKnownIssues = await ghClient.CalculateEstimatedTimeToCloseAllIssuesAsync();

            timeToBetaRelease = TimeSpan.FromSeconds(timeToCloseKnownIssues.TotalSeconds + (timeToCloseImportantIssues.TotalSeconds / numberOfContributors));

            return timeToBetaRelease;
        }

        public async System.Threading.Tasks.Task<TimeSpan> CalculateTimeToFullRelease()
        {
            TimeSpan timeToRelease;
            int numberOfContributors = await ghClient.GetNumberOfActiveContributorsAsync();
            TimeSpan timeToCloseAllCodeIssues = sqClient.CalculateTimeForClosingAllIssues();
            TimeSpan timeToCloseKnownIssues = await ghClient.CalculateEstimatedTimeToCloseAllIssuesAsync();

            timeToRelease = TimeSpan.FromSeconds(timeToCloseKnownIssues.TotalSeconds + (timeToCloseAllCodeIssues.TotalSeconds / numberOfContributors));

            return timeToRelease;
        }
    }
}