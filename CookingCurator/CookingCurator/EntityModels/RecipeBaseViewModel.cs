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
}