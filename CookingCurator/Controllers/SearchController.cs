using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class SearchController : Controller
    {
        private Manager m = new Manager();
        // GET: Search

        [Authorize]
        public ActionResult Search()
        {
            m.isUserBanned();
            if (!m.waiverAccepted())
            {
                return RedirectToAction("AcceptWaiver", "Home", new { Id = m.GetCurrentUserId().ToString(), error = "Please accept the waiver to view recipes and its related features" });
            }
            SearchViewModel search = new SearchViewModel();
            int[] ids = { 1, 2 };
            string[] stringSelection = { "Ingred", "Title" };
            search.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Ingred", Value = "1" },
                new SelectListItem{ Text = "Title", Value = "2" }
            };

            search.searchSelection = stringSelection;
            return View(search);
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel searchModel)
        {
            if (String.IsNullOrWhiteSpace(searchModel.searchString)) {
                string[] stringSelection = { "Ingred", "Title" };
                searchModel.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Ingred", Value = "1", Selected = true },
                new SelectListItem{ Text = "Title", Value = "2" }
            };
                searchModel.searchSelection = stringSelection;
                ModelState.AddModelError("", "Please input text");
                return View("Search",searchModel);
            }

            if (searchModel.searchSelection[0] == "1")
            {
                var recipes = m.searchForRecipe(searchModel);
                string[] stringSelection = { "Ingred", "Title" };
                recipes.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Ingred", Value = "1", Selected = true },
                new SelectListItem{ Text = "Title", Value = "2" }
            };
                recipes.searchSelection = stringSelection;
                return View("Search", recipes);
            }
            else
            {
                var recipes = m.searchByTitle(searchModel);
                string[] stringSelection = { "Ingred", "Title" };
                recipes.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Ingred", Value = "1" },
                new SelectListItem{ Text = "Title", Value = "2", Selected = true }
            };
                recipes.searchSelection = stringSelection;

                return View("Search", recipes);
            }
        }
    }
}