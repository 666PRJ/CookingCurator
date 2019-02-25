using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class IngredientBaseViewModel
    {
        public int ingred_ID { get; set; }
        public string ingred_Name { get; set; }
    }

    public class IngredientBaseUserViewModel
    {
        public int ingred_ID { get; set; }
        public int user_ID { get; set; }
        public string ingred_Name { get; set; }
    }
}