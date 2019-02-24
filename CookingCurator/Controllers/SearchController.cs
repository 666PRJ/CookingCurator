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

        //[Authorize]
        public ActionResult Search()
        {
            SearchViewModel search = new SearchViewModel();
            int[] ids = { 1, 2 };
            string[] stringSelection = { "Search By Ingred", "Search By Title" };
            search.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Search By Ingred", Value = "1" },
                new SelectListItem{ Text = "Search By Title", Value = "2" }
            };

            search.searchSelection = stringSelection;
            return View(search);
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel searchModel)
        {
            if (searchModel.searchSelection[0] == "1")
            {
                var recipes = m.searchForRecipe(searchModel);
                string[] stringSelection = { "Search By Ingred", "Search By Title" };
                recipes.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Search By Ingred", Value = "1", Selected = true },
                new SelectListItem{ Text = "Search By Title", Value = "2" }
            };
                recipes.searchSelection = stringSelection;
                return View("Search", recipes);
            }
            else
            {
                var recipes = m.searchByTitle(searchModel);
                string[] stringSelection = { "Search By Ingred", "Search By Title" };
                recipes.list = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Search By Ingred", Value = "1" },
                new SelectListItem{ Text = "Search By Title", Value = "2", Selected = true }
            };
                recipes.searchSelection = stringSelection;

                return View("Search", recipes);
            }
        }
    }
}