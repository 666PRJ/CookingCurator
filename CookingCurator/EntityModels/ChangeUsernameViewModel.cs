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
        [DisplayName(("User ID Number"))]
        public int user_ID { get; set; }

        [DisplayName(("Username"))]
        [StringLength(30)]
        public string userName { get; set; }

        //[DisplayName(( "New Username"))]
        //public string newUserName { get; set; }
    }
}