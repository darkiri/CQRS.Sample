using System.Web.Mvc;
using CQRS.Sample.Bootstrapping;
using CQRS.Sample.Bus;
using CQRS.Sample.Commands;
using CQRS.Sample.GUI.Models;
using Raven.Client;

namespace CQRS.Sample.GUI.Controllers
{
    public class AccountController : Controller
    {
        readonly IServiceBus _bus;
        IDocumentStore _reportingStore;

        public AccountController(DocumentStoreConfiguration storeConfiguration, IServiceBus bus)
        {
            _bus = bus;
            _reportingStore = storeConfiguration.QueryStore;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return Index();
        }

        [HttpPost]
        public ActionResult Login(AccountDTO model)
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                _bus.Publish(new CreateAccount
                {
                    Email = model.Email,
                    Password = model.Password1,
                });
                _bus.Commit();
                return RedirectToAction("Login");
            } else
            {
                return View(model);
            }
        }
    }
}
