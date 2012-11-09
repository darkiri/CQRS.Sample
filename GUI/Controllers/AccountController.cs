using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CQRS.Sample.Bus;
using CQRS.Sample.Commands;
using CQRS.Sample.GUI.Models;
using CQRS.Sample.Reporting;

namespace CQRS.Sample.GUI.Controllers
{
    public class AccountController : Controller
    {
        readonly AccountQuery _accountQuery;
        readonly IServiceBus _bus;

        public AccountController(AccountQuery accountQuery,
                                 IServiceBus bus)
        {
            _accountQuery = accountQuery;
            _bus = bus;
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginAccountViewModel model, string returnUrl)
        {
            if (ModelState.IsValid && ValidateCredentials(model))
            {
                FormsAuthentication.SetAuthCookie(model.Email, false);
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
                return View(model);
            }
        }

        private bool ValidateCredentials(LoginAccountViewModel model)
        {
            return _accountQuery
                .Execute(model.Email)
                .Any(a => PasswordHash.ValidatePassword(model.Password, a.PasswordHash));
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
                if (AccountAlreadyExists(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "An account with this Email already exists.");
                }
                else
                {
                    _bus.Publish(new CreateAccount
                    {
                        Email = model.Email,
                        Password = model.Password1,
                    });
                    _bus.Commit();
                    return RedirectToAction("Login");
                }
            }
            return View(model);
        }

        bool AccountAlreadyExists(string email)
        {
            return _accountQuery.Execute(email).Any();
        }
    }
}