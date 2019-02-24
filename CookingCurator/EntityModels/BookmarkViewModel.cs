using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookingCurator.EntityModels
{
    public class BookmarkViewModel
    {
        [Key]
        public int recipe_ID { get; set; }
        [Key]
        public int user_ID { get; set; }
        
        public Nullable<bool> bookmarked { get; set; }

        public RecipeBaseViewModel Recipe { get; set; }
    }
}