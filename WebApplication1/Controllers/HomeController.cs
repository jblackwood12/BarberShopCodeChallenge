using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
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