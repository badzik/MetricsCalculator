using MetricsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MetricsApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        void Session_Start(object sender, EventArgs e)
        {
            HttpContext.Current.Session.Add("SessionInfo", new SessionInfo()
            {
                Connected = false,
                ProjectDetails = null
            });
        }


    }
}
