using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class DietController : Controller
    {
        private Manager m = new Manager();

        // GET: Diet
        [Authorize]
        public ActionResult Index()
        {
            var d = m.DietGetAll();
            return View(d);
        }
    }
}