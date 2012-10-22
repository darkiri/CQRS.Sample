using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult Create(CreateAccountModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password1 != model.Password2)
                {
                    ModelState.AddModelError("Password1", "Enter same password twice");
                    ModelState.AddModelError("Password2", "Enter same password twice");
                    return View(model);
                }
                return View();
            } else
            {
                return View(model);
            }
        }
    }

    public class CreateAccountModel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Date)]
        [EmailAddress(ErrorMessage = "Not a valid email")]
        public string Email { get; set; }

        [DisplayName("Password")]
        [Required(ErrorMessage = "Required")]
        public string Password1 { get; set; }

        [DisplayName("Repeat password")]
        [Required(ErrorMessage = "Required")]
        public string Password2 { get; set; }
    }
}
