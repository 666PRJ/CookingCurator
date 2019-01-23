using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class ChangeController : Controller
    {
        // GET: Change
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            ViewBag.Message = "Add/Remove";
            ViewBag.Username = "DoubleHamage";

            ViewBag.Remove = "Remove";
            ViewBag.Add = "Add";

            return View();
        }
    }
}