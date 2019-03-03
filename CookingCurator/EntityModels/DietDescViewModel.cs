using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class DietDescViewModel
    {
        [Key]
        [DisplayName("Diet ID #")]
        public int diet_ID { get; set; }

        [DisplayName("Diet Type")]
        public string dietName { get; set; }

        [DisplayName("Diet Description")]
        public string dietDesc { get; set; }
    }
}