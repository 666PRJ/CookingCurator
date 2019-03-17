using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class HomeController : Controller
    {
        private Manager m = new Manager();

        public ActionResult Index()
        {
            m.isUserBanned();
            return View();
        }

        public ActionResult About()
        {
            m.isUserBanned();
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
                if (!m.IsUsernameSpace(registerModel.userName))
                {
                    ModelState.AddModelError("", "No spaces in Username");
                    return View();
                }

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


        public ActionResult TestAcc()
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
                        return RedirectToAction("AcceptWaiver", new { id = Id });
                    }

                } else if (m.isBanned(loginModel.userEmail)) {
                    ModelState.AddModelError("", "User is banned");
                    return View();
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
        public ActionResult AcceptWaiver(String Id, String error)
        {
            if (!String.IsNullOrEmpty(error))
            {
                ViewBag.error = error;
            }
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

        public ActionResult Forgot()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Forgot(RecoverViewModel recoverModel)
        {

            var u = m.GetUserByEmail(recoverModel.userEmail);

            //Vague message to prevent guessing emails
            if(u == null)
            {
                ModelState.AddModelError("", "An email has been sent to the specified email, provided it was used to make a Cooking Curator Account");
                return View();
            }

            recoverModel.banUser = u.banUser;
            recoverModel.email_Verified = u.email_Verified;
            recoverModel.userName = u.userName;

            bool error = m.RecoverUser(recoverModel);
            if (!error)
            {
                ModelState.AddModelError("", "An email has been sent to the specified email, provided it was used to make a Cooking Curator Account");
                u.GUID = recoverModel.GUID;
                return View();
            }
            else
            {
                //Account is banned or email was never verified
                ModelState.AddModelError("", "The account beloning to the specified email address can not be recovered.");
                return View();
            }

        }

        [Route("reset/{id}")]
        public ActionResult Reset(String id)
        {
            return View();
        }

        [HttpPost]
        [Route("reset/{id}")]
        public ActionResult Reset(RecoverViewModel resetModel, String id)
        {

            //Take the GUID from the URL
            resetModel.GUID = id;

            //Passwords must match
            if(resetModel.confirmPassword != resetModel.password)
            {
                ModelState.AddModelError("", "Error: The passwords you entered do not match.");
                return View();
            }

            //Check for special characters
            Regex r = new Regex("^[a-zA-Z0-9_]*$");
            if (!r.IsMatch(resetModel.password))
            {
                ModelState.AddModelError("", "Error: Do not use special characters, only alphanumeric values.");
                return View();
            }

            if (resetModel.password.Length < 8)
            {
                ModelState.AddModelError("", "Error: Password must be at least 8 characters.");
                return View();
            }

            bool changeSuccess = m.ChangePW(resetModel);

            //Database updated successfully or not
            if(changeSuccess)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Error: An unknown error occured.");
                return View();
            }
        }
    }
}