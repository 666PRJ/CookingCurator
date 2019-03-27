using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ChangeRestrictionsViewModel
    {
        [DisplayName("Select Ingredient Restrictions:")]
        public string[] selectedIngredientsId { get; set; }

        public IEnumerable<IngredientBaseViewModel> allIngredients;

        public IEnumerable<IngredientBaseViewModel> chosenIngredients;
    }
}
