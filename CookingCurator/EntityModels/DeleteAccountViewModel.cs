using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class DeleteAccountViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 8)]
        [DisplayName(("Password"))]
        public string password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 8)]
        [DisplayName(("Confirm Password"))]
        public string ConfirmPassword { get; set; }
    }
}