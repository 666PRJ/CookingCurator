using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace CookingCurator.Models
{
    public partial class DataContext : DbContext
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
        public virtual DbSet<WARN> Warnings { get; set; }

    }
}