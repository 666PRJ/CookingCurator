using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.EntityModels
{
    public class SearchViewModel
    {
        public SearchViewModel() {
            recipeList = new List<RecipeSearchViewModel>();
        }

        [DisplayName("Search")]
        public string searchString { get; set; }

        public List<RecipeSearchViewModel> recipeList;

        public List<SelectListItem> list { get; set; }

        public string[] searchSelection { get; set; }
    }
}