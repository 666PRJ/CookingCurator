using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class AllergyViewModel
    {
        [Key]
        [DisplayName("Allergy ID #")]
        public int allergy_ID { get; set; }

        [DisplayName("Allergy Type")]
        public string allergyName { get; set; }

        [DisplayName("Coverage Information")]
        public string allergyDesc { get; set; }
    }
}