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
        public int user_ID { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        [DisplayName(("Ban User/Unban User"))]
        public bool banUser { get; set; }
    }
}