using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class RecoverViewModel
    {
        [StringLength(30)]
        [DisplayName(("Username"))]
        public string userName { get; set; }

        [StringLength(30)]
        [EmailAddress]
        [DisplayName(("Email Address"))]
        public string userEmail { get; set; }

        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength=8)]
        [DisplayName(("Password"))]
        public string password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 8)]
        [DisplayName(("Confirm Password"))]
        public string confirmPassword { get; set; }

        public bool banUser { get; set; }
        public bool email_Verified { get; set; }

        public string salt { get; set; }
        public string GUID { get; set; }
    }
}