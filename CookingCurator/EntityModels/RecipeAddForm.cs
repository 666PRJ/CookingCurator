using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.EntityModels
{
    public class RecipeAddForm
    {
        public string title { get; set; }

        public string instructions { get; set; }

        public bool verified { get; set; }

        public string source_Link { get; set; }

        public string country { get; set; }

        public string mealTimeType { get; set; }

        public MultiSelectList ingredList { get; set; }
    }
}