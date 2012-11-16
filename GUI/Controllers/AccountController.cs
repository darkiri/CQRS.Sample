using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly AccountQuery _accountQuery;
        private readonly IServiceBus _bus;

        public AccountController(AccountQuery accountQuery,
                                 IServiceBus bus)
        {
            _accountQuery = accountQuery;
            _bus = bus;
        }

        private Guid QueryStreamId(string email)
        {
            return _accountQuery.Execute(email)
                                .First()
                                .StreamId;
        }

        private bool ValidateCredentials(string email, string password)
        {
            return _accountQuery
                .Execute(email)
                .Any(a => PasswordHash.ValidatePassword(password, a.PasswordHash));
        }

        private bool AccountAlreadyExists(string email)
        {
            return _accountQuery.Execute(email) .Any();
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginAccountViewModel model, string returnUrl)
        {
            if (ModelState.IsValid && ValidateCredentials(model.Email, model.Password))
            {
                Response.SetAuthCookie(model.Email, false, QueryStreamId(model.Email));

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
                    var createAccount = new CreateAccount
                    {
                        Email = model.Email,
                        Password = model.Password1,
                    };
                    _bus.PublishWithLatency(createAccount);
                    return RedirectToAction("Login");
                }
            }
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidateCredentials(User.Identity.Name, model.Password))
                {
                    _bus.PublishWithLatency(new ChangePassword(HttpContext.GetStreamId())
                    {
                        OldPassword = model.Password,
                        NewPassword = model.Password1,
                    });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Password", "Password is incorrect");
                }
            }
            return View(model);
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }


    public static class BusExtensions
    {
        public static void PublishWithLatency(this IServiceBus bus, IMessage message)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                bus.Publish(message);
                bus.Commit();
            });
        }
    }
}