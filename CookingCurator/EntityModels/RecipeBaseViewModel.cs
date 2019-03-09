using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace CookingCurator.EntityModels
{
    public class RecipeWithMatchedIngred : RecipeBaseViewModel
    {
        [DisplayName("Matched Ingredients")]
        public int matchedIngredients { get; set; }
    }

    public class RecipeBaseViewModel : RecipeAddViewModel
    {
        [Key]
        public int recipe_Id { get; set; }

    }

    public class RecipeWithIngredBaseViewModel : RecipeBaseViewModel
    {
        public RecipeWithIngredBaseViewModel()
        {
            ingreds = new List<IngredientBaseViewModel>();
        }

        public IEnumerable<RecipeBaseViewModel> recommended { get; set; }

        public IEnumerable<IngredientBaseViewModel> ingreds { get; set; }

        public byte[] Content { get; set; }

        public string Content_Type { get; set; }

        public string fileResult { get; set; }
    }

}