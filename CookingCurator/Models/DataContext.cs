using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CookingCurator.Models
{
    public partial class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext()
            : base("name=prj666_191a03Entities")
        {
        }

        public virtual DbSet<RECIPE_INGREDS> Recipe_Ingreds { get; set; }
        public virtual DbSet<RECIPE> Recipes { get; set; }
        public virtual DbSet<ALLERGY> Allergies { get; set; }
        public virtual DbSet<BMK> BMKs { get; set; }
        public virtual DbSet<DIET> Diets { get; set; }
        public virtual DbSet<INGRED> Ingreds { get; set; }
        public virtual DbSet<USER> Users { get; set; }
        public virtual DbSet<ALLERGY_INGREDS> Allergy_ingreds { get; set; }
        public virtual DbSet<DIET_INGREDS> Diet_Ingreds { get; set; }
        public virtual DbSet<DIET_RECIPES> Diet_Recipes { get; set; }
        public virtual DbSet<RECIPE_USERS> Recipe_Users { get; set; }
    }
}