using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class SearchViewModel
    {
        public SearchViewModel() {
            recipeList = new List<RecipeBaseViewModel>();
        }
        public string searchString { get; set; }

        public List<RecipeBaseViewModel> recipeList;
    }
}