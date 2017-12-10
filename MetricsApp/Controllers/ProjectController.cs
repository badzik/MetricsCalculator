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
using MetricsApp.Metrics;
using MetricsApp.SQJsonModels;
using Newtonsoft.Json;

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
                //Preparing model and metrics object
                GitHubMetrics ghMetrics = new GitHubMetrics(sessionInfo.ProjectDetails.GitHubProjectName, sessionInfo.ProjectDetails.GitHubProjectOwner,sessionInfo.ProjectDetails.GitHubToken);
                IssuesModel model = new IssuesModel();

                model.AverageIssueClosingTime = await ghMetrics.CalculateAverageIssueClosingTimeAsync();
                model.ClosedIssues = await ghMetrics.CountClosedIssuesAsync();
                model.OpenedIssues = await ghMetrics.CountOpenedIssuesAsync();
                model.EstimatedTimeToCloseAllIssues = await ghMetrics.CalculateEstimatedTimeToCloseAllIssuesAsync();
                model.ExpectedDateForClosingAllIssues = DateTime.Now + model.EstimatedTimeToCloseAllIssues;
                model.ClosedIssuesForMonth = await ghMetrics.CountClosedIssuesForLastSixMonthsAsync();
                model.UserWithLargestIssuesClosed = await ghMetrics.FindUserWithLargestNumberOfClosedIssuesAsync();

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
    }
}