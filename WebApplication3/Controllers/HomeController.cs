using System.Web.Mvc;

namespace WebApplication3.Controllers
{
	public class HomeController : Controller
	{
		[HttpGet]
		public ActionResult Index()
		{
			return View("Index");
		}
	}
}