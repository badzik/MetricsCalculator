using MetricsApp.Helpers;
using MetricsApp.SQJsonModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MetricsApp.Models;
using MetricsApp.Enums;

namespace MetricsApp.Metrics
{
    public class SonarQubeMetrics
    {
        public string ProjectName { get; set; }
        public string ServerUrl { get; set; }

        public SonarQubeMetrics(string projectName, string serverUrl)
        {
            ProjectName = projectName;
            ServerUrl = serverUrl;
        }

        public int CountBugs()
        {
            int bugsCounter=0;
            var restClient = new RestClient(ServerUrl + "/api/measures/component?metricKeys=bugs&component=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            JsonComponent component = JsonConvert.DeserializeObject<JsonComponent>(response.Content);
            bugsCounter = (int)component.Component.Measures[0].Value;
            return bugsCounter;
        }

        public int GetNumberOfAllLines()
        {
            int linesCount=0;
            var restClient = new RestClient(ServerUrl + "/api/measures/component?metricKeys=ncloc&component=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            JsonComponent component = JsonConvert.DeserializeObject<JsonComponent>(response.Content);
            linesCount = (int)component.Component.Measures[0].Value;
            return linesCount;
        }

        public int GetNumberOfMinorIssues()
        {
            int counter = 0;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY,CODE_SMELL&severities=MINOR,INFO");
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            counter = issues.TotalIssues;
            return counter;
        }

        public int GetNumberOfMajorIssues()
        {
            int counter = 0;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY,CODE_SMELL&severities=BLOCKER,CRITICAL,MAJOR");
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            counter = issues.TotalIssues;
            return counter;
        }

        public int CountVulnerabilities()
        {
            int vulCounter=0;
            var restClient = new RestClient(ServerUrl + "/api/measures/component?metricKeys=vulnerabilities&component=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            JsonComponent component = JsonConvert.DeserializeObject<JsonComponent>(response.Content);
            vulCounter = (int)component.Component.Measures[0].Value;
            return vulCounter;
        }

        public TimeSpan CalculateAverageTimeForResolvingIssue()
        {
            TimeSpan time = new TimeSpan();
            int counter = 0;
            int page = 1;
            int numberOfPages = 1;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            numberOfPages = (int)Math.Ceiling((Double)issues.TotalIssues / issues.PageSize);
            while (numberOfPages >= page)
            {
                restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&p=" + page);
                request = new RestRequest();
                request.Method = Method.GET;
                request.AddHeader("Accept", "application/json");
                request.Parameters.Clear();
                response = restClient.Execute(request);
                issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
                foreach (Issue i in issues.Issues)
                {
                    if (i.effort != null && i.effort != "")
                    {
                        time += Parser.EffortParser(i.effort);
                        counter++;
                    }
                }
                page++;
            }
            if (counter == 0)
            {
                return TimeSpan.FromSeconds(0);
            }
            return TimeSpan.FromSeconds(time.TotalSeconds / counter);
        }

        //Summarize effort for closing major,critical and blocker issues
        public TimeSpan CalculateTimeForClosingAllImportantIssues()
        {
            TimeSpan time = new TimeSpan();
            TimeSpan avgResolvTime = CalculateAverageTimeForResolvingIssue();
            int page = 1;
            int numberOfPages = 1;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY,CODE_SMELL&severities=BLOCKER,CRITICAL,MAJOR");
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            numberOfPages = (int)Math.Ceiling((Double)issues.TotalIssues / issues.PageSize);
            while (numberOfPages >= page)
            {
                restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY&p=" + page);
                request = new RestRequest();
                request.Method = Method.GET;
                request.AddHeader("Accept", "application/json");
                request.Parameters.Clear();
                response = restClient.Execute(request);
                issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
                foreach (Issue i in issues.Issues)
                {
                    if (i.effort != null && i.effort != "")
                    {
                        time += Parser.EffortParser(i.effort);
                    }
                    else
                    {
                        time += avgResolvTime;
                    }

                }
                page++;
            }
            return time;
        }

        public CodeQualityFactors CalculateCodeQuality()
        {
            CodeQualityFactors cqFactors = new CodeQualityFactors();
            int duplicatedLines = GetNumberOfDuplicatedLines();
            int allLines = GetNumberOfAllLines();
            double? coverage = GetPercentageCoverage();
            int projectComplexity = GetProjectComplexity();
            int numberOfMajorIssues = GetNumberOfMajorIssues();

            double maxAllowedComplexity = Math.Round((double)allLines / 10);
            double maxAllowedMajorIssues = Math.Round((double)allLines / 1000);

            double percentageUncoverage = coverage.HasValue ? coverage.Value : 100.0;
            double complexity= projectComplexity > maxAllowedComplexity ? 1 : (double)projectComplexity / maxAllowedComplexity;
            double majorIssuesRatio = numberOfMajorIssues > maxAllowedMajorIssues ? 1 : (double)numberOfMajorIssues / maxAllowedMajorIssues;

            cqFactors.ComplexityRatio = Math.Round(10.0 * (double)complexity);
            cqFactors.DuplicatedLinesRatio = Math.Round(20.0 * ((double)duplicatedLines / allLines));
            cqFactors.MajorIssuesRatio = Math.Round(50.0 * (double)majorIssuesRatio);
            cqFactors.TestUncoverageRatio = Math.Round(20.0 * ((double)percentageUncoverage / 100));
            cqFactors.CodeQuality = 100 - ((double)cqFactors.TestUncoverageRatio + cqFactors.DuplicatedLinesRatio + cqFactors.ComplexityRatio + cqFactors.MajorIssuesRatio);

            return cqFactors;
        }

        public int GetProjectComplexity()
        {
            int complexity = 0;
            var restClient = new RestClient(ServerUrl + "/api/measures/component?metricKeys=complexity&component=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            JsonComponent component = JsonConvert.DeserializeObject<JsonComponent>(response.Content);
            complexity = (int)component.Component.Measures[0].Value;
            return complexity;
        }

        //Summarize time effort to fix bugs and vulnerabilites
        public TimeSpan CalculateTimeForClosingBugsAndVulnerabilitiesIssues()
        {
            TimeSpan time = new TimeSpan();
            TimeSpan avgResolvTime = CalculateAverageTimeForResolvingIssue();
            int page = 1;
            int numberOfPages = 1;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY");
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            numberOfPages = (int)Math.Ceiling((Double)issues.TotalIssues / issues.PageSize);
            while (numberOfPages >= page)
            {
                restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY&p=" + page);
                request = new RestRequest();
                request.Method = Method.GET;
                request.AddHeader("Accept", "application/json");
                request.Parameters.Clear();
                response = restClient.Execute(request);
                issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
                foreach (Issue i in issues.Issues)
                {
                    if (i.effort != null && i.effort != "")
                    {
                        time += Parser.EffortParser(i.effort);
                    }
                    else
                    {
                        time += avgResolvTime;
                    }

                }
                page++;
            }
            return time;
        }

        //Summarize Time to close all issues(with code smells)
        public TimeSpan CalculateTimeForClosingAllIssues()
        {
            TimeSpan time = new TimeSpan();
            TimeSpan avgResolvTime = CalculateAverageTimeForResolvingIssue();
            int page = 1;
            int numberOfPages = 1;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            numberOfPages = (int)Math.Ceiling((Double)issues.TotalIssues / issues.PageSize);
            while (numberOfPages >= page)
            {
                restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&p=" + page);
                request = new RestRequest();
                request.Method = Method.GET;
                request.AddHeader("Accept", "application/json");
                request.Parameters.Clear();
                response = restClient.Execute(request);
                issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
                foreach (Issue i in issues.Issues)
                {
                    if (i.effort != null && i.effort != "")
                    {
                        time += Parser.EffortParser(i.effort);
                    }
                    else
                    {
                        time += avgResolvTime;
                    }
                }
                page++;
            }
            return time;
        }

        internal List<IssueCountWithSeverity> GetIssuesCountWithSeverity()
        {
            List<IssueCountWithSeverity> issuesList = new List<IssueCountWithSeverity>();

            int page = 1;
            int numberOfPages = 1;
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            numberOfPages = (int)Math.Ceiling((Double)issues.TotalIssues / issues.PageSize);
            while (numberOfPages >= page)
            {
                restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&p=" + page);
                request = new RestRequest();
                request.Method = Method.GET;
                request.AddHeader("Accept", "application/json");
                request.Parameters.Clear();
                response = restClient.Execute(request);
                issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
                foreach (Issue i in issues.Issues)
                {
                    if (issuesList.Exists(x => x.Severity.ToString().Equals(i.severity)))
                    {
                        issuesList.Find(x => x.Severity.ToString().Equals(i.severity)).Counter++;
                    }
                    else
                    {
                        issuesList.Add(new IssueCountWithSeverity()
                        {
                            Counter = 1,
                            Severity = (Severity)Enum.Parse(typeof(Severity), i.severity)
                        });

                    }
                }
                page++;
            }
            return issuesList;
        }

        public int GetNumberOfDuplicatedLines()
        {
            int duplicatedLines = 0;
            var restClient = new RestClient(ServerUrl + "/api/measures/component?metricKeys=duplicated_lines&component=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            JsonComponent component = JsonConvert.DeserializeObject<JsonComponent>(response.Content);
            duplicatedLines = (int)component.Component.Measures[0].Value;
            return duplicatedLines;
        }

        public Double? GetPercentageCoverage()
        {
            Double? coverage = null;
            var restClient = new RestClient(ServerUrl + "/api/measures/component?metricKeys=coverage&component=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            JsonComponent component = JsonConvert.DeserializeObject<JsonComponent>(response.Content);
            if (component.Component.Measures.Count > 0)
            {
                coverage = component.Component.Measures[0].Value;
            }
            return coverage;
        }


    }
}