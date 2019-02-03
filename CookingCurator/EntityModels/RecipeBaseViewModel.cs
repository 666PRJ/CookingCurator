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
}