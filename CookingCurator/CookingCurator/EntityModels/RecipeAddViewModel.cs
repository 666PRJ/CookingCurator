using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CookingCurator.EntityModels
{
    public class RecipeAddViewModel
    {
       
        public string title { get; set; }

       
        public int rating { get; set; }

       
        public string instructions { get; set; }

        public System.DateTime lastUpdated { get; set; }


        public string author { get; set; }

        public bool verified { get; set; }

        public Nullable<int> source_ID { get; set; }

        public string source_Link { get; set; }

        
        public string country { get; set; }

    
        public string mealTimeType { get; set; }
    }
}