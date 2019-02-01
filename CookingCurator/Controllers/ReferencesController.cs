using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class ReferencesController : Controller
    {
        private Manager m = new Manager();
        // GET: References
        public ActionResult Index()
        {
            var t = m.RecipeSourceGetAll();
            return View(t);
        }


    }
}