using MetricsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MetricsApp.Controllers
{
    public class InfoController : Controller
    {
        // GET: Information
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Information(string text)
        {
            InfoModel model = new InfoModel();
            if (text == "SuccesfulConnect")
            {
                model.Text = "Succesfully connected to all tools. Visit metrics page for more details.";
                model.ImgPath = "/Graphics/connected.png";
            }
            if(text== "Disconnected")
            {
                model.Text = "You have sucessfully disconnected from all project instruments.";
                model.ImgPath = "/Graphics/disconnected.png";
            }
            return View(model);
        }
    }
}