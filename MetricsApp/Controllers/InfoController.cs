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
            string temp = "No Info";
            if (text == "SuccesfulConnect")
            {
                temp = "Succesfully connected to all tools. Visit metrics page for more details.";
            }
            return View((object)temp);
        }
    }
}