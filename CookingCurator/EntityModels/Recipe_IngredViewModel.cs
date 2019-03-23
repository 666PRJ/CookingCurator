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
    public class Recipe_IngredViewModel : RecipeBaseViewModel
    {
        [Required]
        [DisplayName("Ingredient Selection")]
        public string[] selectedIngredsId { get; set; }

        public IEnumerable<IngredientBaseViewModel> ingredients;

        public byte[] Content { get; set; }

        public string Content_Type { get; set; }

        public string fileResult { get; set; }

        [DisplayName("Diet Types")]
        public string[] selectedDietsId { get; set; }

        public IEnumerable<DietDescViewModel> diets;

    }
}