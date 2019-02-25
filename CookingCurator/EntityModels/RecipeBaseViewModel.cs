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

        public int user_Id { get; set; }
    }

    public class RecipeWithIngredBaseViewModel : RecipeBaseViewModel
    {
        public RecipeWithIngredBaseViewModel()
        {
            ingreds = new List<IngredientBaseViewModel>();
        }

        public IEnumerable<IngredientBaseViewModel> ingreds { get; set; }
    }

    public class RecipeWithIngredBaseUserViewModel : RecipeBaseViewModel
    {
        public RecipeWithIngredBaseUserViewModel()
        {
            ingreds = new List<IngredientBaseViewModel>();
        }

        public string user_Id { get; set; }
        public IEnumerable<IngredientBaseViewModel> ingreds { get; set; }
    }
}