using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace CookingCurator.EntityModels
{
    public class Recipe_IngredViewModel : RecipeWithIngredBaseViewModel
    { 
        [DisplayName("Ingredient Selection")]
        public string[] selectedIngredsId { get; set; }

        public IEnumerable<IngredientBaseViewModel> ingredients;

    }
}