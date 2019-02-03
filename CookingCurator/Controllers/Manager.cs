using AutoMapper;
using CookingCurator.EntityModels;
using CookingCurator.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CookingCurator.Controllers
{
    public class Manager
    {
        // Reference to the data context
        private DataContext ds = new DataContext();

        // AutoMapper components
        MapperConfiguration config;
        public IMapper mapper;

        public object Response { get; private set; }

        public Manager()
        {
            // If necessary, add more constructor code here...

            // Configure the AutoMapper components
            config = new MapperConfiguration(cfg =>
            {
                // Define the mappings below, for example...
                // cfg.CreateMap<SourceType, DestinationType>();
                // cfg.CreateMap<Employee, EmployeeBase>();
                cfg.CreateMap<RECIPE, RecipeBaseViewModel>();

                cfg.CreateMap<RECIPE, RecipeSourceViewModel>();

                cfg.CreateMap<USER, UserBaseViewModel>();

                cfg.CreateMap<RecipeAddViewModel, RECIPE>();

                cfg.CreateMap<UserFindViewModel, USER>();

                cfg.CreateMap<INGRED, IngredientBaseViewModel>();
            });

            mapper = config.CreateMapper();

            // Turn off the Entity Framework (EF) proxy creation features
            // We do NOT want the EF to track changes - we'll do that ourselves
            ds.Configuration.ProxyCreationEnabled = false;

            // Also, turn off lazy loading...
            // We want to retain control over fetching related objects
            ds.Configuration.LazyLoadingEnabled = false;
        }

        // Add methods below
        // Controllers will call these methods
        // Ensure that the methods accept and deliver ONLY view model objects and collections
        // The collection return type is almost always IEnumerable<T>

        // Suggested naming convention: Entity + task/action
        // For example:
        // ProductGetAll()
        // ProductGetById()
        // ProductAdd()
        // ProductEdit()
        // ProductDelete()



        public IEnumerable<RecipeBaseViewModel> RecipeGetAll()
        {
            // The ds object is the data store
            // It has a collection for each entity it manages

            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(ds.Recipes);
        }

        public RecipeBaseViewModel RecipeGetById(int? id)
        {
            // Attempt to fetch the object.
            var recipe = ds.Recipes.Find(id);

            // Return the result (or null if not found).
            return recipe == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(recipe);
        }

        public RecipeBaseViewModel RecipeAdd(RecipeAddViewModel recipe)
        {
            // Attempt to add the new item.
            // Notice how we map the incoming data to the Customer design model class.
            var addedItem = ds.Recipes.Add(mapper.Map<RecipeAddViewModel, RECIPE>(recipe));
            ds.SaveChanges();

            // If successful, return the added item (mapped to a view model class).
            return addedItem == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(addedItem);
        }

        public RecipeBaseViewModel RecipeEdit(Recipe_IngredViewModel recipeIng)
        {
            var recipeUpdate = ds.Recipes.Find(recipeIng.recipe.recipe_Id);
            if (recipeUpdate == null)
            {
                return null;
            }
            recipeUpdate.title = recipeIng.recipe.title;
            recipeUpdate.instructions = recipeIng.recipe.instructions;
            recipeUpdate.lastUpdated = DateTime.Now;
            recipeUpdate.author = recipeIng.recipe.author;
            recipeUpdate.source_Link = recipeIng.recipe.source_Link;
            recipeUpdate.country = recipeIng.recipe.country;
            recipeUpdate.mealTimeType = recipeIng.recipe.mealTimeType;
            ds.Entry(recipeUpdate).State = System.Data.Entity.EntityState.Modified;

            deleteIngredients(recipeIng.recipe.recipe_Id);
            addIngredients(recipeIng.recipe.recipe_Id, recipeIng.selectedIngredsId);
            // Attempt to save the edited recipe.
            ds.SaveChanges();

            // If successful, return the edited recipe (mapped to a view model class).
            return recipeUpdate == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(recipeUpdate);
        }

        public void RecipeDelete(int id)
        {
            deleteIngredients(id);
            var recipe = ds.Recipes.Find(id);
            ds.Recipes.Remove(recipe);
            ds.SaveChanges();
        }

        public IEnumerable<IngredientBaseViewModel> IngredientGetAll()
        {
            return mapper.Map<IEnumerable<INGRED>, IEnumerable<IngredientBaseViewModel>>(ds.Ingreds);
        }

        public void addIngredients(int id, String[] selectedIds)
        {
            for(int i = 0; i < selectedIds.Length; i++)
            {
                String query = "INSERT INTO RECIPE_INGREDS (recipe_ID, ingred_ID) VALUES (" + id + "," + Int32.Parse(selectedIds[i]) + ")";
                ds.Database.ExecuteSqlCommand(query);
            }
            ds.SaveChanges();
        }

        public List<String> ingredsForRecipe(int? id)
        {
            List<String> selectedIngreds = new List<string>();
            IEnumerable<RECIPE_INGREDS> ingreds = ds.Recipe_Ingreds.SqlQuery("Select * from RECIPE_INGREDS where recipe_Id = " + id);
            foreach(var item in ingreds)
            {
                selectedIngreds.Add(item.ingred_ID.ToString());
            }
            return selectedIngreds;
        }
        public void deleteIngredients(int id)
        {
            ds.Database.ExecuteSqlCommand("delete from RECIPE_INGREDS where recipe_Id = " + id);
            ds.SaveChanges();
        }

        public IEnumerable<UserBaseViewModel> UserFindAll()
        {
            return mapper.Map<IEnumerable<USER>, IEnumerable<UserBaseViewModel>>(ds.Users);
        }

        public IEnumerable<UserBaseViewModel> UserFind(UserFindViewModel find) {
            var findItem = ds.Users.Where(t => t.userName.Contains(find.userName));

            return findItem == null ? null : mapper.Map<IEnumerable<USER>, IEnumerable<UserBaseViewModel>>(findItem);
        }


        public IEnumerable<UserBaseViewModel> BanUserById(int id)
        {
            // Attempt to fetch the object.
            var obj = ds.Users.SingleOrDefault(e => e.user_ID == id);

            obj.banUser = !obj.banUser;

            ds.SaveChanges();



            // Return the result (or null if not found).
            return mapper.Map<IEnumerable<USER>, IEnumerable<UserBaseViewModel>>(ds.Users);
        }

        public IEnumerable<RecipeSourceViewModel> RecipeSourceGetAll()
        {
            // The ds object is the data store
            // It has a collection for each entity it manages

            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeSourceViewModel>>(ds.Recipes.Where(t => t.source_ID.HasValue == true));
        }

    }
}