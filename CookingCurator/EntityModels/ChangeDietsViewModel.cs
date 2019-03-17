using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ChangeDietsViewModel
    {
        [DisplayName("Select Diet Types:")]
        public string[] selectedDietsId { get; set; }

        public IEnumerable<DietDescViewModel> allDiets;

        public IEnumerable<DietDescViewModel> chosenDiets;
    }
}
