using MetricsApp.Models;
using Newtonsoft.Json.Linq;
using Octokit;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MetricsApp.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
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
                ModelState.AddModelError("","Cannot connect to GitHub repository, check GitHub details");
                return View(projectDetails);
            }


            //Check connection to SonarQube
            var restClient = new RestClient(projectDetails.SonarServerUrl+"/api/components/show?component="+projectDetails.SonarProjectName);
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
        
    }
}