using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class RegisterViewModel
    {
        [StringLength(30)]
        public string userName { get; set; }

        [StringLength(30)]
        [Required]
        public string userEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength=8)]
        public string password { get; set; }

        public bool acceptWaiver { get; set; }
        public bool banUser { get; set; }
        public bool email_Verified { get; set; }
        public string salt { get; set; }
        public string GUID { get; set; }
    }
}