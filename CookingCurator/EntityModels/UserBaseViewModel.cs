using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class UserBaseViewModel
    {
        [DisplayName(("User ID Number"))]
        public int user_ID { get; set; }

        [DisplayName(("Username"))]
        public string userName { get; set; }

        [DisplayName(("Email Address"))]
        public string userEmail { get; set; }

        [DisplayName(("Ban User/Unban User"))]
        public bool banUser { get; set; }
    }
}