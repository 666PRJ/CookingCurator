using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace CookingCurator.EntityModels
{
    public class RecipeSourceViewModel
    {
        [DisplayName("Source ID #")]
        public int source_ID { get; set; }

        [DisplayName("Source Link")]
        public string source_Link { get; set; }
    }
}