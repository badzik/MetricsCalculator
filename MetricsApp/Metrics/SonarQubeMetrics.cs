using MetricsApp.Helpers;
using MetricsApp.SQJsonModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Metrics
{
    public class SonarQubeMetrics
    {
        public string ProjectName { get; set; }
        public string ServerUrl { get; set; }

        public SonarQubeMetrics(string projectName,string serverUrl)
        {
            ProjectName = projectName;
            ServerUrl = serverUrl;
        }

       public int CountBugs()
        {
            int bugsCounter;
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

        public int CountVulnerabilities()
        {
            int vulCounter;
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
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            foreach (Issue i in issues.Issues)
            {
                if (i.effort != null && i.effort != "")
                {
                    time += Parser.EffortParser(i.effort);
                    counter++;
                }
            }
            return TimeSpan.FromSeconds(time.TotalSeconds / counter);
        }

        //Summarize time effort to fix bugs and vulnerabilites
        public TimeSpan CalculateTimeForClosingImportantIssues()
        {
            TimeSpan time = new TimeSpan();
            TimeSpan avgResolvTime = CalculateAverageTimeForResolvingIssue();
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName + "&types=BUG,VULNERABILITY");
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
            foreach(Issue i in issues.Issues)
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
            return time;
        }

        //Summarize Time to close all issues(with code smells)
        public TimeSpan CalculateTimeForClosingAllIssues()
        {
            TimeSpan time = new TimeSpan();
            TimeSpan avgResolvTime = CalculateAverageTimeForResolvingIssue();
            var restClient = new RestClient(ServerUrl + "/api/issues/search?componentKeys=" + ProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            IRestResponse response = restClient.Execute(request);
            IssuesList issues = JsonConvert.DeserializeObject<IssuesList>(response.Content);
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
            return time;
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
            duplicatedLines =(int)component.Component.Measures[0].Value;
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