using MetricsApp.Models;
using Newtonsoft.Json.Linq;
using Octokit;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MetricsApp.ViewModels;

namespace MetricsApp.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConnectToProject()
        {
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> ConnectToProject(ProjectDetails projectDetails)
        {
            if (!ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                foreach (var modelStateVal in ViewData.ModelState.Values)
                {
                    foreach (var error in modelStateVal.Errors)
                    {
                        var errorMessage = error.ErrorMessage;
                        var exception = error.Exception;
                        errors.Add(errorMessage);
                    }
                }
                foreach (string error in errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(projectDetails);
            }

            //Check connection to github
            GitHubClient client;
            Credentials tokenAuth;
            client = new GitHubClient(new ProductHeaderValue("MetricsApp"));
            tokenAuth = new Credentials(projectDetails.GitHubToken);
            client.Credentials = tokenAuth;
            try
            {
                Repository repo = await client.Repository.Get(projectDetails.GitHubProjectOwner, projectDetails.GitHubProjectName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Cannot connect to GitHub repository, check GitHub details");
                return View(projectDetails);
            }


            //Check connection to SonarQube
            var restClient = new RestClient(projectDetails.SonarServerUrl + "/api/components/show?component=" + projectDetails.SonarProjectName);
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();

            IRestResponse response = restClient.Execute(request);
            if (response.Content == "")
            {
                ModelState.AddModelError("", "Wrong SonarQube server URL");
                return View(projectDetails);
            }
            JObject data = JObject.Parse(response.Content);
            if (data["errors"] != null)
            {
                ModelState.AddModelError("", "Cannot connect to project on SonarQube server, check project name");
                return View(projectDetails);
            }
            if (data["component"] != null)
            {
                HttpContext.Session["SessionInfo"] = new SessionInfo()
                {
                    Connected = true,
                    ProjectDetails = projectDetails
                };
            }

            return RedirectToAction("Information", "Info", new { text = "SuccesfulConnect" });
        }

        public ActionResult Disconnect()
        {
            HttpContext.Session["SessionInfo"] = new SessionInfo()
            {
                Connected = false,
                ProjectDetails = null
            };

            return RedirectToAction("Information", "Info", new { text = "Disconnected" });
        }

        public async System.Threading.Tasks.Task<ActionResult> Issues()
        {
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Session["SessionInfo"];
            if (!sessionInfo.Connected)
            {
                return RedirectToAction("ConnectToProject", "Project");
            }
            else
            {
                //Preparing model and gh client
                IssuesModel model = new IssuesModel();
                GitHubClient ghClient = prepareGitHubClient();

                //Get all closed issues and calculate avg. closing time
                var closedIssues = await ghClient.Issue.GetAllForRepository(sessionInfo.ProjectDetails.GitHubProjectOwner, sessionInfo.ProjectDetails.GitHubProjectName, new RepositoryIssueRequest() { State = ItemStateFilter.Closed});
                TimeSpan totalTime = new TimeSpan();
                foreach (Issue i in closedIssues)
                {
                    totalTime += i.ClosedAt.Value.Subtract(i.CreatedAt);
                }
                model.AverageIssueClosingTime = TimeSpan.FromSeconds(totalTime.TotalSeconds / closedIssues.Count);

                //get opened issues
                var openedIssues = await ghClient.Issue.GetAllForRepository(sessionInfo.ProjectDetails.GitHubProjectOwner, sessionInfo.ProjectDetails.GitHubProjectName, new RepositoryIssueRequest() { State = ItemStateFilter.Open });

                model.ClosedIssues = closedIssues.Count;
                model.OpenedIssues = openedIssues.Count;

                //Calculate estimated time to close all issues based on avg. closing time and number of active contributors
                var contributors = await ghClient.Repository.GetAllContributors(sessionInfo.ProjectDetails.GitHubProjectOwner, sessionInfo.ProjectDetails.GitHubProjectName);
                int numberOfActiveContributors = 0;
                foreach (RepositoryContributor c in contributors)
                {
                    if (c.Contributions > 0)
                    {
                        numberOfActiveContributors++;
                    }
                }
                model.EstimatedTimeToCloseAllIssues = TimeSpan.FromSeconds((closedIssues.Count / numberOfActiveContributors) * model.AverageIssueClosingTime.TotalSeconds);

                model.ExpectedDateForClosingAllIssues = DateTime.Now + model.EstimatedTimeToCloseAllIssues;

                //Count closed issues for last six months, where [5] - this month, [4] - this.month -1 ...
                model.ClosedIssuesForMonth = new List<IssueCountForMonth>();
                int currentMonth = DateTime.Now.Month;
                int startMonth = DateTime.Now.AddMonths(-6).Month;

                for (int i = startMonth + 1; i <= currentMonth; i++)
                {
                    model.ClosedIssuesForMonth.Add(new IssueCountForMonth()
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
                        model.ClosedIssuesForMonth[monthDifference].Issues += 1;
                    }
                }

                //Find the user with the largest number of closed issues, BUG ON OCKTOKIT
                List<UserWithCounter> users = new List<UserWithCounter>();
                foreach (Issue i in closedIssues)
                {
                    //Getting Issues one by one becasue when getting all closed by attribute is null
                    var closedIssue = await ghClient.Issue.Get(sessionInfo.ProjectDetails.GitHubProjectOwner, sessionInfo.ProjectDetails.GitHubProjectName, i.Number);
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
                model.UserWithLargestIssuesClosed = UserWithCounter.GetUserWithLargestCounter(users);

                return View(model);
            }

        }

        public ActionResult CodeQuality()
        {
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Session["SessionInfo"];
            if (!sessionInfo.Connected)
            {
                return RedirectToAction("ConnectToProject", "Project");
            }
            else
            {
                //calculate metrics
                return View();
            }

        }

        private GitHubClient prepareGitHubClient()
        {
            GitHubClient client;
            Credentials tokenAuth;
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Session["SessionInfo"];
            client = new GitHubClient(new ProductHeaderValue("MetricsApp"));
            tokenAuth = new Credentials(sessionInfo.ProjectDetails.GitHubToken);
            client.Credentials = tokenAuth;
            return client;
        }

        public static int MonthDifference(DateTime lValue, DateTime rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }
    }
}