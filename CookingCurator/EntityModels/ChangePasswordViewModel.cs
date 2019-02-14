using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ChangePasswordViewModel
    {
        [StringLength(30, MinimumLength = 8)]
        [DisplayName(("Password"))]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [StringLength(30, MinimumLength = 8)]
        [DisplayName(("Confirm Password"))]
        [DataType(DataType.Password)]
        public string confirmPassword { get; set; }
    }
}