using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class BookmarkController : Controller
    {
        private Manager m = new Manager();
        // GET: Bookmark
        public ActionResult Index(string error)
        {
            m.isUserBanned();
            if (!String.IsNullOrEmpty(error))
            {
                ViewBag.error = error;
            }
            return View(m.GetAllBookmarks());
        }

        // GET: Bookmark/Details/5
        public ActionResult Details(int iD)
        {
            m.isUserBanned();
            return RedirectToAction("Details", "Recipe", new { id = iD} );
        }

        public ActionResult Delete(int? id)
        {
            if(id == null)
            {
                return RedirectToAction("Index");
            }
            bool error =  m.DeleteBookmark(id.GetValueOrDefault());
            if (!error)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", new { error = "Error deleting a bookmark" });
            }
        }

        public ActionResult DeleteAll()
        {
            bool error = m.DeleteAllBookmarks();
            if (!error)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", new { error = "Error deleting all bookmarks" });
            }
        }
    }
}
