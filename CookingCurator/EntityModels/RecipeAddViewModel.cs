using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CookingCurator.EntityModels
{
    public class RecipeIngred
    {
        public int recipe_ID { get; set; }
        public int ingred_ID { get; set; }
    }

    public class RecipeAddViewForm
    {
        [Required]
        public string title { get; set; }


        public int rating { get; set; }

        [Required]
        public string instructions { get; set; }

        public System.DateTime lastUpdated { get; set; }

        [Required]
        public string author { get; set; }

        public bool verified { get; set; }

        public Nullable<int> source_ID { get; set; }

        public string source_Link { get; set; }

        [Required]
        public string country { get; set; }

        [Required]
        public string mealTimeType { get; set; }

        public string[] selectedIngredsId { get; set; }
        public IEnumerable<IngredientBaseViewModel> ingredients;
    }

    public class RecipeAddViewModel
    {
        [Required]
        public string title { get; set; }


        public int rating { get; set; }

        [Required]
        public string instructions { get; set; }

        public System.DateTime lastUpdated { get; set; }

        [Required]
        public string author { get; set; }

        public bool verified { get; set; }

        public Nullable<int> source_ID { get; set; }

        public string source_Link { get; set; }

        [Required]
        public string country { get; set; }

        [Required]
        public string mealTimeType { get; set; }

    }

    public class RecipeVerifiedAddViewModel
    {
        [Required]
        public string title { get; set; }


        public int rating { get; set; }

        [Required]
        public string instructions { get; set; }

        public System.DateTime lastUpdated { get; set; }

        [Required]
        public string author { get; set; }

        public bool verified { get; set; }

        public Nullable<int> source_ID { get; set; }

        [Required]
        public string source_Link { get; set; }

        [Required]
        public string country { get; set; }

        [Required]
        public string mealTimeType { get; set; }

        public string[] selectedIngredsId { get; set; }
        public IEnumerable<IngredientBaseViewModel> ingredients;
    }
}