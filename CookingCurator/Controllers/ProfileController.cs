using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//Will be necessary once login is enabled

namespace CookingCurator.Controllers
{
    public class ProfileController : Controller
    {
        private Manager m = new Manager();
        //For Admins (naming convention must change whe we have admins and users)
        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard()
        {
            return View();
        }

        //For Non-Admins 
        [Authorize]
        public ActionResult UserDashboard()
        {
            return View();
        }

        [Authorize]
        public ActionResult ContactUs()
        {
            return View();
        }

        [Authorize]
        public ActionResult ChangeUsername()
        {
            ChangeUsernameViewModel username = new ChangeUsernameViewModel();
            username.userName = "";
            return View(username);
        }

        [HttpPost]
        public ActionResult ChangeUsername(ChangeUsernameViewModel newUsername)
        {
            if (!ModelState.IsValid)
            {
                return View(newUsername);
            }

            bool isDup = m.IsDupUserName(newUsername);

            if (isDup == false) {
                newUsername.ErrorMessage = "Username taken";
                return View(newUsername);
            }

            bool error = m.ChangeUsername(newUsername);
            if (error == false) {
                newUsername.ErrorMessage = "Error with username";
                return View(newUsername);
            }
            else {
                return Redirect("UserDashboard");
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel newPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(newPassword);
            }

            bool error = m.ChangePassword(newPassword);
            if (error == false) {
                newPassword.ErrorMessage = "Error with Password";
                return View(newPassword);
            }
            else {
                return Redirect("UserDashboard");
            }
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();

            model.ErrorMessage = "";

            return View(model);
        }

        [HttpPost]
        public ActionResult ContactUs(ContactUsViewModel contactUs)
        {
            if (!ModelState.IsValid)
                return View(contactUs);

            bool error = m.ContactAdmin(contactUs);

            if (!error)
            {
                return View(contactUs);
            }
            else
            {
                return RedirectToAction("UserDashboard");
            }

        }
    }
}