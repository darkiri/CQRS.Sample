using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using CQRS.Sample.GUI.Models;

namespace CQRS.Sample.GUI.Controllers
{
    public class AccountController : Controller
    {
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
                return RedirectToAction("Login");
            } else
            {
                return View(model);
            }
        }
    }
}
