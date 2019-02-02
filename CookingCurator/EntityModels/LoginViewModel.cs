using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class LoginViewModel
    {
        public int user_ID { get; set; }
        [DisplayName("Username/Email")]
        public string userEmail { get; set; }
        public string password { get; set; }
    }
}