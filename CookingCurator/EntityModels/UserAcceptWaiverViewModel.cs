using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class UserAcceptWaiverViewModel
    {
        [Key]
        public int user_ID { get; set; }
    }
}