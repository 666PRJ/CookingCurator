using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class HomeController : Controller
    {
        private Manager m = new Manager();

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel registerModel)
        {
            if (ModelState.IsValid)
            {
                bool error = m.RegisterUser(registerModel);
                if (!error)
                {
                    return RedirectToAction("RegisterationSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Username or Password");
                    return View();
                }
            }
            else
            {
                return View(registerModel);
            }

        }

        public ActionResult RegisterationSuccess()
        {
            return View();
        }
        [HttpGet]
        public ActionResult VerifyAccount(String id)
        {
            m.AccountVerification(id);

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                bool error = m.LoginUser(loginModel);
                if (!error)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Username or Password");
                    return View();
                }
            }
            else
            {
                return View(loginModel);
            }

        }

        public ActionResult LogOut()
        {
            bool error = m.logoutUser();
            if (!error)
            {
                return Redirect("Login");
            }
            else
            {
                return Redirect("Index");
            }

        }
    }
}