using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class ChangeUsernameViewModel
    {
        [StringLength(30, MinimumLength = 8)]
        [DisplayName(("Password"))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayName(("Username"))]
        [StringLength(30)]
        public string userName { get; set; }

        public string ErrorMessage { get; set; }
    }
}