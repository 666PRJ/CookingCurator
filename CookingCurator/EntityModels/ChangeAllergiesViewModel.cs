using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ChangeAllergiesViewModel
    {
        [DisplayName("Select Allergies:")]
        public string[] selectedAllergiesId { get; set; }

        public IEnumerable<AllergyViewModel> allAllergies;

        public IEnumerable<AllergyViewModel> chosenAllergies;
    }
}