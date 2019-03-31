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

        [Authorize(Roles = "Admin")]
        public ActionResult AdminDoc()
        {
            return View();
        }

        [Authorize]
        public ActionResult DeleteAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteAccount(DeleteAccountViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            var result = m.AccountDelete(user);
            if (result)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid Password");
                return View(user);
            }
        }

        //For Non-Admins 
        [Authorize]
        public ActionResult UserDashboard()
        {
            m.isUserBanned();
            return View();
        }

        [Authorize]
        public ActionResult UserDoc()
        {
            m.isUserBanned();
            return View();
        }

        [Authorize]
        public ActionResult ContactUs()
        {
            m.isUserBanned();
            String email = m.GetCurrentUserEmail();
            ContactUsViewModel model = new ContactUsViewModel();
            model.emailAddress = email;
            return View(model);
        }

        [Authorize]
        public ActionResult ChangeUsername()
        {
            m.isUserBanned();
            ChangeUsernameViewModel username = new ChangeUsernameViewModel();
            username.userName = "";
            return View(username);
        }

        [HttpPost]
        public ActionResult ChangeUsername(ChangeUsernameViewModel newUsername)
        {
            m.isUserBanned();
            if (!ModelState.IsValid)
            {
                return View(newUsername);
            }



            if (!m.IsUsernameSpace(newUsername.userName))
            {
                ModelState.AddModelError("", "No spaces/special characters in username");
                return View(newUsername);
            }

            bool isDup = m.IsDupUserName(newUsername);

            if (isDup == false) {
                ModelState.AddModelError("", "Username taken");
                return View(newUsername);
            }

            bool error = m.ChangeUsername(newUsername);
            if (error == false) {
                ModelState.AddModelError("", "Error with username");
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
                ModelState.AddModelError("", "Error with Password");
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
                ViewBag.success = "An email is sent to the administrator. Thanks for contacting us, we will get back to you!";
                return View();
            }

        }

        // GET: Profile/ChangeDiets
        [Authorize]
        public ActionResult ChangeDiets()
        {
            string userCheck = m.GetCurrentUsername();
            int idNum = m.FetchUserId(userCheck);

            IEnumerable<DietDescViewModel> diets = m.DietsForUserProfile(idNum);
            String[] selectedDiets = m.DietsForUser(idNum).ToArray();

            ChangeDietsViewModel dietChange = new ChangeDietsViewModel();

            dietChange.allDiets = m.DietsForChangeScreen();
            dietChange.chosenDiets = diets;
            dietChange.selectedDietsId = selectedDiets;

            return View(dietChange);
        }

        // POST: Profile/ChangeDiets
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeDiets(ChangeDietsViewModel updateDiets)
        {
            string userCheck = m.GetCurrentUsername();
            int idNum = m.FetchUserId(userCheck);

            m.DeleteDietsForUser(idNum);
            if (updateDiets.selectedDietsId != null)
            {
                m.UpdateDietsForUser(idNum, updateDiets.selectedDietsId);
            }

            updateDiets.allDiets = m.DietsForChangeScreen();
            updateDiets.chosenDiets = m.DietsForUserProfile(idNum);
            return View(updateDiets);
        }

        // GET: Profile/ChangeAllergies
        [Authorize]
        public ActionResult ChangeAllergies()
        {
            string userCheck = m.GetCurrentUsername();
            int idNum = m.FetchUserId(userCheck);

            IEnumerable<AllergyViewModel> allergies = m.AllergiesForUserProfile(idNum);
            String[] selectedAllergies = m.AllergiesForUser(idNum).ToArray();

            ChangeAllergiesViewModel allergyChange = new ChangeAllergiesViewModel();

            allergyChange.allAllergies = m.AllergyGetAll();
            allergyChange.chosenAllergies = allergies;
            allergyChange.selectedAllergiesId = selectedAllergies;

            return View(allergyChange);
        }

        // POST: Profile/ChangeAllergies
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAllergies(ChangeAllergiesViewModel updateAllergies)
        {
            string userCheck = m.GetCurrentUsername();
            int idNum = m.FetchUserId(userCheck);

            m.DeleteAllergiesForUser(idNum);
            if (updateAllergies.selectedAllergiesId != null)
            {
                m.UpdateAllergiesForUser(idNum, updateAllergies.selectedAllergiesId);
            }

            updateAllergies.allAllergies = m.AllergyGetAll();
            updateAllergies.chosenAllergies = m.AllergiesForUserProfile(idNum);
            return View(updateAllergies);
        }

        // GET: Profile/ChangeRestrictions
        [Authorize]
        public ActionResult ChangeRestrictions()
        {
            string userCheck = m.GetCurrentUsername();
            int idNum = m.FetchUserId(userCheck);

            IEnumerable<IngredientBaseViewModel> restrictions = m.IngredsForUserProfile(idNum);
            String[] selectedRestrictions = m.IngredsForUser(idNum).ToArray();

            ChangeRestrictionsViewModel resChange = new ChangeRestrictionsViewModel();

            resChange.allIngredients = m.IngredGetAll();
            resChange.chosenIngredients = restrictions;
            resChange.selectedIngredientsId = selectedRestrictions;

            return View(resChange);
        }

        // POST: Profile/ChangeRestrictions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRestrictions(ChangeRestrictionsViewModel updateRes)
        {
            string userCheck = m.GetCurrentUsername();
            int idNum = m.FetchUserId(userCheck);

            m.DeleteIngredsForUser(idNum);
            if (updateRes.selectedIngredientsId != null)
            {
                m.UpdateIngredsForUser(idNum, updateRes.selectedIngredientsId);
            }

            updateRes.allIngredients = m.IngredGetAll();
            updateRes.chosenIngredients = m.IngredsForUserProfile(idNum);
            return View(updateRes);
        }
    }
}