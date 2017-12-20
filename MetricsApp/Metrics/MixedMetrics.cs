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

            Double maxNumberOfMinorIssues = allLines / 100;
            pFactors.CodeQualityRatio = 0.6;
            if ((openedIssues+closedIssues)>0)
            {
                pFactors.KnownIssuesRatio = (openedIssues / (closedIssues + openedIssues));
            }
            else
            {
                pFactors.KnownIssuesRatio = 0;
            }

            pFactors.MinorIssuesRatio = numberOfMinorIssues > maxNumberOfMinorIssues ? 1 : numberOfMinorIssues / maxNumberOfMinorIssues;

            pFactors.ProjectQuality = 100-((pFactors.CodeQualityRatio * (100-cqFactors.CodeQuality)) + (((pFactors.KnownIssuesRatio * 70) + (pFactors.MinorIssuesRatio*30))*0.4));


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