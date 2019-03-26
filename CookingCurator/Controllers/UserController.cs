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
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var u = m.UserFindAll();
            return View(u);
        }

        [Route("User/Search")]
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        public ActionResult BanUser(int? id)
        {
            var u = m.BanUserById(id.GetValueOrDefault());

            return View("Index", u);
        }

        // GET: User/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            var u = m.GetUserById(id.GetValueOrDefault());

            if (u == null)
                return HttpNotFound();
            else
                return View(u);
        }
    }
}