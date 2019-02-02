using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ContactUsViewModel
    {
        [DisplayName(("Email Address"))]
        [Required]
        public string emailAddress { get; set; }

        [DisplayName(("Feed Back"))]
        [Required]
        public string feedBack { get; set; }
    }
}