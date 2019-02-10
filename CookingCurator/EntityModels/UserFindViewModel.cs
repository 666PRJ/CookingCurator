using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace CookingCurator.EntityModels
{
    public class UserFindViewModel
    {
        [DisplayName(("Username"))]
        public string userName { get; set; }
    }
}