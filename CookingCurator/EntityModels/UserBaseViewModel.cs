using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class UserBaseViewModel
    {
        public int user_ID { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public Nullable<int> admin_ID { get; set; }
    }
}