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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search()
        {
            SearchViewModel search = new SearchViewModel();
            return View(search);
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel searchModel)
        {
            var recipes = m.searchForRecipe(searchModel);
            return View("Search",recipes);
        }
    }
}