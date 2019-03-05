using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class AllergyController : Controller
    {
        private Manager m = new Manager();

        // GET: Allergy
        [Authorize]
        public ActionResult Index()
        {
            m.isUserBanned();
            var a = m.AllergyGetAll();
            return View(a);
        }

        [Authorize]
        public ActionResult Ingredient(string allergyName)
        {
            m.isUserBanned();
            if (allergyName == null){
                return RedirectToAction("Index");
            }

            var i = m.GetIngredByAllergen(allergyName);
            ViewBag.Allergy = allergyName;
            return View(i);
        }
    }
}