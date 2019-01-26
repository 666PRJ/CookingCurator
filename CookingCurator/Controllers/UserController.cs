using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class UserController : Controller
    {
        private Manager m = new Manager();

        // GET: User
        public ActionResult Index()
        {
            var u = m.UserFindAll();
            return View(u);
        }

        [Route("User/Search")]
        public ActionResult Search()
        {
            return View();
        }

        [Route("User/Search")]
        [HttpPost]
        public ActionResult Search(UserFindViewModel newItem)
        {
            var u = m.UserFind(newItem);

            return View("Index", u);
        }
    }
}