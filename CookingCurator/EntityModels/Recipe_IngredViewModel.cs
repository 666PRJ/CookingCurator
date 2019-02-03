using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class Recipe_IngredViewModel
    {
        public RecipeBaseViewModel recipe { get; set; }
        public string[] selectedIngredsId { get; set; }
        public IEnumerable<IngredientBaseViewModel> ingredients;
    }
}