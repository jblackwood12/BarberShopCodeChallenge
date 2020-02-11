using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
	public class HomeController : Controller
	{
		[HttpGet]
		public ActionResult Index()
		{
			return View(new ScheduleCustomerModel());
		}
	}
}