using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class LoginViewModel
    {
        public int user_ID { get; set; }

        [DisplayName("Username/Email")]
        public string userEmail { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string password { get; set; }
    }
}