using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApplication3
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			// Issue with minifying Angular
			BundleTable.EnableOptimizations = false;
		}

		protected void Session_start(object sender, EventArgs e)
		{
			// Extend session timeout to 60 minutes.
			Session.Timeout = 60;
		}
	}
}
