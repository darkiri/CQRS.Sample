using System.Web.Mvc;

namespace CQRS.Sample.GUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        public ActionResult ChangePassword()
        {
            return RedirectToAction("ChangePassword", "Account");
        }
    }
}
