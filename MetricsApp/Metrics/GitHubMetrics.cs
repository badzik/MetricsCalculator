using MetricsApp.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MetricsApp.Metrics
{
    public class GitHubMetrics
    {
        private GitHubClient client;
        public string ProjectName { get; set; }
        public string ProjectOwner { get; set; }
        private string authToken { get; set; }
        private Boolean basicMetricsInit;
        private IReadOnlyList<Issue> closedIssues;
        private IReadOnlyList<Issue> openedIssues;
        private IReadOnlyList<RepositoryContributor> contributors;

        public GitHubMetrics(string projectName,string projectOwner,string authToken)
        {
            ProjectName = projectName;
            ProjectOwner = projectOwner;
            this.authToken = authToken;
            Credentials tokenAuth;
            client = new GitHubClient(new ProductHeaderValue("MetricsApp"));
            tokenAuth = new Credentials(authToken);
            client.Credentials = tokenAuth;
            basicMetricsInit = false;
        }

        private async Task<bool> prepareBasicMetrics()
        {
            closedIssues = await client.Issue.GetAllForRepository(ProjectOwner, ProjectName, new RepositoryIssueRequest() { State = ItemStateFilter.Closed });
            openedIssues= await client.Issue.GetAllForRepository(ProjectOwner, ProjectName, new RepositoryIssueRequest() { State = ItemStateFilter.Open });
            contributors= await client.Repository.GetAllContributors(ProjectOwner, ProjectName);
            basicMetricsInit = true;
            return true;
        }

        public async System.Threading.Tasks.Task<TimeSpan> CalculateAverageIssueClosingTimeAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            if (closedIssues.Count == 0)
            {
                return TimeSpan.FromSeconds(0);
            }
            TimeSpan totalTime = new TimeSpan();
            TimeSpan avgIssueClosingTime;
            foreach (Issue i in closedIssues)
            {
                totalTime += i.ClosedAt.Value.Subtract(i.CreatedAt);
            }
            avgIssueClosingTime = TimeSpan.FromSeconds(totalTime.TotalSeconds / closedIssues.Count);
            return avgIssueClosingTime;
        }

        public async Task<int> CountOpenedIssuesAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            return openedIssues.Count;
        }

        public async Task<int> CountClosedIssuesAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            return closedIssues.Count;
        }

        public async Task<int> GetNumberOfActiveContributorsAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            int numberOfActiveContributors = 0;
            foreach (RepositoryContributor c in contributors)
            {
                if (c.Contributions > 0)
                {
                    numberOfActiveContributors++;
                }
            }
            return numberOfActiveContributors;
        }

        //Calculate estimated time to close all issues based on avg. closing time and number of active contributors
        public async Task<TimeSpan> CalculateEstimatedTimeToCloseAllIssuesAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            int numberOfActiveContributors = await GetNumberOfActiveContributorsAsync();
            TimeSpan avgIssueClosingTime = await CalculateAverageIssueClosingTimeAsync();
            if (numberOfActiveContributors == 0)
            {
                return TimeSpan.FromSeconds(0);
            }
            return TimeSpan.FromSeconds((closedIssues.Count /numberOfActiveContributors) *(avgIssueClosingTime.TotalSeconds));
        }

        //Count closed issues for last six months, where [5] - this month, [4] - this.month -1 ...
        public async Task<List<IssueCountForMonth>> CountClosedIssuesForLastSixMonthsAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            List<IssueCountForMonth> closedIssuesForMonth = new List<IssueCountForMonth>();
            int currentMonth = DateTime.Now.Month;
            int startMonth = DateTime.Now.AddMonths(-6).Month;

            for (int i = startMonth + 1; i <= currentMonth; i++)
            {
                closedIssuesForMonth.Add(new IssueCountForMonth()
                {
                    Issues = 0,
                    MonthName = CultureInfo.GetCultureInfo("en-US").DateTimeFormat.GetMonthName(i)
                });
            }

            foreach (Issue i in closedIssues)
            {
                int monthDifference = MonthDifference(DateTime.Now, i.ClosedAt.Value.DateTime);
                if (monthDifference < 6)
                {
                    closedIssuesForMonth[monthDifference].Issues += 1;
                }
            }
            return closedIssuesForMonth;
        }

        public async System.Threading.Tasks.Task<UserWithCounter> FindUserWithLargestNumberOfClosedIssuesAsync()
        {
            if (!basicMetricsInit)
            {
                await prepareBasicMetrics();
            }
            List<UserWithCounter> users = new List<UserWithCounter>();
            foreach (Issue i in closedIssues)
            {
                //Getting Issues one by one becasue when getting all closed by attribute is null
                var closedIssue = await client.Issue.Get(ProjectOwner, ProjectName, i.Number);
                if (closedIssue.ClosedBy != null)
                {
                    //if user exist in list increment his counter
                    if (users.Where(u => u.User.Id.Equals(closedIssue.ClosedBy.Id)).Any())
                    {
                        users.Find(u => u.User.Id == closedIssue.ClosedBy.Id).Counter++;
                    }
                    //if not create new user in list
                    else
                    {
                        users.Add(new UserWithCounter()
                        {
                            Counter = 1,
                            User = closedIssue.ClosedBy
                        });
                    }
                }
            }
            return UserWithCounter.GetUserWithLargestCounter(users);
        }

        public static int MonthDifference(DateTime lValue, DateTime rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }
    }
}