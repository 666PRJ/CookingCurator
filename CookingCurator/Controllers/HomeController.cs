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
                    int Id = m.FetchUserId(registerModel.userName);
                    return RedirectToAction("RegisterationSuccess", new { id = Id.ToString() });
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

        [Route("registration/success/{id}")]
        public ActionResult RegisterationSuccess(String id)
        {
            ViewBag.MyString = id;
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
                    int Id = m.FetchUserId(loginModel.userEmail);
                    bool waiverAccepted = m.IsWaiverAccepted(Id);
                    if (waiverAccepted)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("AcceptWaiver", new { id = Id});
                    }
                    
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

        [HttpGet]
        public ActionResult AcceptWaiver(String Id)
        {
            int id = Convert.ToInt32(Id);
            UserBaseViewModel user = m.GetUserById(id);
            UserAcceptWaiverViewModel acceptWaiverUser = m.mapper.Map<UserBaseViewModel, UserAcceptWaiverViewModel>(user);
            return View(acceptWaiverUser);
        }

        [HttpPost]
        public ActionResult AcceptWaiver(UserAcceptWaiverViewModel user)
        {
            bool error = m.AcceptWaiverByUser(user);
            if (!error)
            {
                return View(user.user_ID.ToString());
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}