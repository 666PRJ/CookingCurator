using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class RecipeBaseViewModel : RecipeAddViewModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int recipe_Id { get; set; }
    }

    public class RecipeWithIngredBaseViewModel : RecipeBaseViewModel
    {
        public RecipeWithIngredBaseViewModel()
        {
            ingreds = new List<IngredBase>();
        }

        public IEnumerable<IngredBase> ingreds { get; set; }
    }
}