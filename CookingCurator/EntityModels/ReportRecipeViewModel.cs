using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ReportRecipeViewModel
    {
        [Key]
        [Required]
        public int recipeId { get; set; }

        [DisplayName(("User Name"))]
        [ReadOnly(true)]
        public string userName { get; set; }

        [DisplayName(("Recipe Being Reported"))]
        [ReadOnly(true)]
        public string recipeTitle { get; set; }

        [DisplayName(("Feed Back"))]
        [Required(ErrorMessage =  "Feedback is required")]
        public string feedBack { get; set; }
    }
}