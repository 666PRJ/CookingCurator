using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//Will be necessary once login is enabled

namespace CookingCurator.Controllers
{
    public class ProfileController : Controller
    {
        //For Admins (naming convention must change whe we have admins and users)
        public ActionResult Dashboard()
        {
            return View();
        }

        //For Non-Admins 
        public ActionResult UserDashboard()
        {
            return View();
        }
    }
}