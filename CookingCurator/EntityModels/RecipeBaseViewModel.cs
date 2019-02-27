using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CookingCurator.EntityModels
{
    public class RecipeBaseViewModel : RecipeAddViewModel
    {
        [Key]
        public int recipe_Id { get; set; }
    }

    public class RecipeSearchViewModel : RecipeAddViewModel
    {
        [Key]
        public int recipe_Id { get; set; }

        public int sentIngred { get; set; }

        public int totalIngred { get; set; }
    }

    public class RecipeWithIngredBaseViewModel : RecipeBaseViewModel
    {
        public RecipeWithIngredBaseViewModel()
        {
            ingreds = new List<IngredientBaseViewModel>();
        }
        public IEnumerable<IngredientBaseViewModel> ingreds { get; set; }
    }
}