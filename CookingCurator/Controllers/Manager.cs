using AutoMapper;
using CookingCurator.EntityModels;
using CookingCurator.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

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

                cfg.CreateMap<RECIPE, RecipeWithIngredBaseViewModel>();

                cfg.CreateMap<RecipeBaseViewModel, RECIPE>();

                cfg.CreateMap<RECIPE, RecipeSourceViewModel>();

                cfg.CreateMap<RecipeVerifiedAddViewModel, RECIPE>();

                cfg.CreateMap<USER, UserBaseViewModel>();

                cfg.CreateMap<RecipeAddViewForm, RECIPE>();

                cfg.CreateMap<RecipeIngred, RECIPE>();

                cfg.CreateMap<RECIPE, RecipeWithMatchedIngred>();

                cfg.CreateMap<UserFindViewModel, USER>();

                cfg.CreateMap<INGRED, IngredientBaseViewModel>();

                cfg.CreateMap<RegisterViewModel, USER>();

                cfg.CreateMap<UserBaseViewModel, UserAcceptWaiverViewModel>();

                cfg.CreateMap<UserAcceptWaiverViewModel, USER>();

                cfg.CreateMap<USER, ChangeUsernameViewModel>();

                cfg.CreateMap<USER, ChangePasswordViewModel>();

                cfg.CreateMap<USER, RecoverViewModel>();

                cfg.CreateMap<RECIPE_USERS, BookmarkViewModel>();

                cfg.CreateMap<ALLERGY, AllergyViewModel>();

                cfg.CreateMap<AllergyViewModel, ALLERGY>();

                cfg.CreateMap<DIET, DietDescViewModel>();

                cfg.CreateMap<DietDescViewModel, DIET>();

                cfg.CreateMap<RecipeWithIngredBaseViewModel, Recipe_IngredViewModel>();

                cfg.CreateMap<RECIPE, RecipeWithImagesViewModel>();

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


        public List<RecipeBaseViewModel> giveRecommendations(List<String> ingreds, int id)
        {
            List<RECIPE_INGREDS> recipesIngreds = new List<RECIPE_INGREDS>();
            List<RECIPE> recipes = new List<RECIPE>();
            List<RECIPE> finalRecipes = new List<RECIPE>();
            foreach (var item in ingreds)
            {
                IEnumerable<RECIPE_INGREDS> bridge = ds.Recipe_Ingreds.SqlQuery("Select * from RECIPE_INGREDS where ingred_Id = " + item);
                recipesIngreds.AddRange(bridge);

            }

            foreach (var item in recipesIngreds)
            {
                IEnumerable<RECIPE> derp = ds.Recipes.Where(e => e.recipe_ID == item.recipe_ID);
                recipes.AddRange(derp);
            }

            recipes = recipes.Distinct().ToList();

            recipes = recipes.Where(x => x.recipe_ID != id).ToList();

            recipes = recipes.Take(3).ToList();

            return mapper.Map<List<RECIPE>, List<RecipeBaseViewModel>>(recipes);
        }

        public bool isBanned(string username) {
            USER current = ds.Users.SingleOrDefault(e => e.userName == username);

            if (current != null) {
                return current.banUser;
            }

            return false;

        }

        public bool isUserBanned()
        {
            string username = HttpContext.Current.User.Identity.Name;

            if (String.IsNullOrEmpty(username)) {
                return false;
            }

            USER current = ds.Users.SingleOrDefault(e => e.userName == username);

            if (current.banUser) {
                FormsAuthentication.SignOut();
            }

            return current.banUser;
        }

        public ChangeUsernameViewModel GetUsername()
        {
            string username = HttpContext.Current.User.Identity.Name;
            USER current = ds.Users.SingleOrDefault(e => e.userName == username);

            return current == null ? null : mapper.Map<USER, ChangeUsernameViewModel>(current);
        }

        public ChangeUsernameViewModel GetPassword()
        {
            string username = HttpContext.Current.User.Identity.Name;
            USER current = ds.Users.SingleOrDefault(e => e.userName == username);

            return current == null ? null : mapper.Map<USER, ChangeUsernameViewModel>(current);
        }

        public bool IsDupUserName(ChangeUsernameViewModel newUsername) {
            var user = ds.Users.SingleOrDefault(e => e.userName == HttpContext.Current.User.Identity.Name);
            var duplicateFound = ds.Users.Where(f => f.userName == newUsername.userName);
            if (user == null)
            {
                return false;
            }
            if (duplicateFound.Count() > 0)
            {
                return false;
            }

            return true;
        }

        public bool IsUsernameSpace(string username) {
            Regex r = new Regex("^[a-zA-Z0-9_]*$");
            if (!r.IsMatch(username))
            {
                return false;
            }
            return true;
        }

        public bool IsUsernameDup(string username)
        {
            var duplicateFound = ds.Users.Where(f => f.userName == username);
            if (duplicateFound == null)
            {
                return false;
            }
            if (duplicateFound.Count() > 0)
            {
                return false;
            }

            return true;
        }

        public bool IsEmailDup(string email)
        {
            var duplicateFound = ds.Users.Where(f => f.userEmail == email);
            if (duplicateFound == null)
            {
                return false;
            }
            if (duplicateFound.Count() > 0)
            {
                return false;
            }

            return true;
        }

        public bool ChangeUsername(ChangeUsernameViewModel newUsername) {
            var user = ds.Users.SingleOrDefault(e => e.userName == HttpContext.Current.User.Identity.Name);
            var duplicateFound = ds.Users.Where(f => f.userName == newUsername.userName);
            if (user == null) {
                return false;
            }
            if (duplicateFound.Count() > 0) {
                return false;
            }

            var userRecipe = ds.Recipes.Where(e => e.author == user.userName);

            foreach (var item in userRecipe) {
                item.author = newUsername.userName;
                ds.Entry(item).State = System.Data.Entity.EntityState.Modified;
            }


            user.userName = newUsername.userName;
            ds.Entry(user).State = System.Data.Entity.EntityState.Modified;

            ds.SaveChanges();

            FormsAuthentication.SignOut();
            FormsAuthentication.SetAuthCookie(newUsername.userName, false);
            return true;
        }

        public bool ChangePassword(ChangePasswordViewModel newPassword)
        {
            var user = ds.Users.SingleOrDefault(e => e.userName == HttpContext.Current.User.Identity.Name);
            if (user == null)
            {
                return false;
            }
            var pass = HashPasswordLogin(newPassword.OldPassword, user.salt);
            if (user.password != pass)
            {
                return false;
            }

            Regex r = new Regex("^[a-zA-Z0-9_]*$");
            if (!r.IsMatch(newPassword.password))
            {
                return false;
            }

            if (newPassword.password != newPassword.confirmPassword) {
                return false;
            }

            var hashedPw = HashPasswordLogin(newPassword.password, user.salt);
            String query = "UPDATE USERS SET password = \"" + hashedPw + "\" WHERE userEmail = \"" + user.userEmail + "\"";
            ds.Database.ExecuteSqlCommand(query);
            ds.SaveChanges();

            FormsAuthentication.SignOut();
            FormsAuthentication.SetAuthCookie(user.userName, false);
            return true;
        }

        public IEnumerable<RecipeBaseViewModel> RecipeGetAll()
        {
            // The ds object is the data store
            // It has a collection for each entity it manages
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(ds.Recipes);
        }

        public IEnumerable<RecipeWithImagesViewModel> RecipeGetAllWithImages()
        {
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes);
        }

        public IEnumerable<RecipeWithImagesViewModel> RecipeGetFilteredByDietWithImages(int idNum)
        {
            var recipes = ds.Recipes.SqlQuery("Select recipe_Id, title, rating, instructions, lastUpdated, author, verified, source_ID, source_Link, country, mealTimeType, content, 'Content-Type' AS 'Content_Type' FROM RECIPES WHERE recipe_ID IN (SELECT recipe_ID FROM DIET_RECIPES WHERE diet_ID IN (SELECT diet_Id FROM USER_DIETS WHERE user_Id =" + idNum + "))");

            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(recipes);
        }

        public RecipeBaseViewModel RecipeGetById(int? id)
        {
            // Attempt to fetch the object.
            var recipe = ds.Recipes.Find(id);

            // Return the result (or null if not found).
            return recipe == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(recipe);
        }

        public RecipeWithIngredBaseViewModel RecipeWithIngredGetById(int? id)
        {
            // Attempt to fetch the object.
            var recipe = ds.Recipes.Find(id);

            // Return the result (or null if not found).
            return recipe == null ? null : mapper.Map<RECIPE, RecipeWithIngredBaseViewModel>(recipe);
        }

        public IEnumerable<RecipeBaseViewModel> RecipesByAuthor(string byAuthor)
        {
            var authorRecipes = ds.Recipes.Where(r => r.author == byAuthor);

            return authorRecipes == null ? null : mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(authorRecipes);
        }

        public IEnumerable<DietDescViewModel> DietGetAll()
        {
            return mapper.Map<IEnumerable<DIET>, IEnumerable<DietDescViewModel>>(ds.Diets);
        }

        public IEnumerable<RecipeBaseViewModel> GetRecipesByDiet(string diet_Name)
        {
            var diet_num = ds.Diets.SingleOrDefault(d => d.dietName == diet_Name);

            var recipes = ds.Recipes.SqlQuery("Select recipe_Id, title, rating, instructions, lastUpdated, author, verified, source_ID, source_Link, country, mealTimeType, content, 'Content-Type' AS 'Content_Type' FROM RECIPES WHERE recipe_ID IN (SELECT recipe_ID FROM DIET_RECIPES WHERE diet_ID = " + diet_num.diet_ID + ")");

            return recipes == null ? null : mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(recipes);
        }


        public RecipeBaseViewModel RecipeAdd(RecipeAddViewForm recipe)
        {
            recipe.author = HttpContext.Current.User.Identity.Name;
            var addedItem = ds.Recipes.Add(mapper.Map<RecipeAddViewForm, RECIPE>(recipe));

            //Ingredient management
            deleteIngredients(addedItem.recipe_ID);
            addIngredientsForRecipes(addedItem.recipe_ID, recipe.selectedIngredsId);

            //Diet management
            deleteDiets(addedItem.recipe_ID);
            addDietsForRecipes(addedItem.recipe_ID, recipe.selectedDietsId);

            ds.SaveChanges();

            // If successful, return the added item (mapped to a view model class).
            return addedItem == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(addedItem);
        }

        public void createRecipeIngred(IEnumerable<int> ingredIdList, int id)
        {
            foreach (var item in ingredIdList)
            {
                RecipeIngred recipe_ingreds = new RecipeIngred();
                recipe_ingreds.recipe_ID = id;
                recipe_ingreds.ingred_ID = item;
                var derp = ds.Recipe_Ingreds.Add(mapper.Map<RecipeIngred, RECIPE_INGREDS>(recipe_ingreds));
            }

            ds.SaveChanges();
        }

        public RecipeBaseViewModel RecipeVerifiedAdd(RecipeVerifiedAddViewModel recipe)
        {

            var addedItem = ds.Recipes.Add(mapper.Map<RecipeVerifiedAddViewModel, RECIPE>(recipe));

            //Ingredient management
            deleteIngredients(addedItem.recipe_ID);
            addIngredientsForRecipes(addedItem.recipe_ID, recipe.selectedIngredsId);

            //Diet management
            deleteDiets(addedItem.recipe_ID);
            addDietsForRecipes(addedItem.recipe_ID, recipe.selectedDietsId);

            ds.SaveChanges();

            // If successful, return the added item (mapped to a view model class).
            return addedItem == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(addedItem);
        }

        public RecipeBaseViewModel RecipeIDUpdate(RecipeBaseViewModel recipe)
        {

            // Notice how we map the incoming data to the Customer design model class.
            var recipeUpdate = ds.Recipes.Find(recipe.recipe_Id);
            if (recipeUpdate.source_Link != "")
            {
                recipeUpdate.source_ID = recipeUpdate.recipe_ID;
            }
            ds.Entry(recipeUpdate).State = System.Data.Entity.EntityState.Modified;

            ds.SaveChanges();

            // If successful, return the added item (mapped to a view model class).
            return recipeUpdate == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(recipeUpdate);
        }

        public RecipeBaseViewModel RecipeEdit(Recipe_IngredViewModel recipeIng)
        {
            var recipeUpdate = ds.Recipes.Find(recipeIng.recipe_Id);
            if (recipeUpdate == null)
            {
                return null;
            }
            recipeUpdate.title = recipeIng.title;
            recipeUpdate.instructions = recipeIng.instructions;
            recipeUpdate.lastUpdated = DateTime.Now;
            recipeUpdate.author = recipeIng.author;
            recipeUpdate.source_Link = recipeIng.source_Link;
            recipeUpdate.country = recipeIng.country;
            recipeUpdate.mealTimeType = recipeIng.mealTimeType;
            recipeUpdate.Content = recipeIng.Content;
            recipeUpdate.Content_Type = recipeIng.Content_Type;
            ds.Entry(recipeUpdate).State = System.Data.Entity.EntityState.Modified;

            deleteIngredients(recipeIng.recipe_Id);
            if (recipeIng.selectedIngredsId != null)
            {
                addIngredientsForRecipes(recipeIng.recipe_Id, recipeIng.selectedIngredsId);
            }
            deleteDiets(recipeIng.recipe_Id);
            if (recipeIng.selectedDietsId != null)
            {
                addDietsForRecipes(recipeIng.recipe_Id, recipeIng.selectedDietsId);
            }

            // Attempt to save the edited recipe.
            ds.SaveChanges();

            // If successful, return the edited recipe (mapped to a view model class).
            return recipeUpdate == null ? null : mapper.Map<RECIPE, RecipeBaseViewModel>(recipeUpdate);
        }

        public void RecipeDelete(int id)
        {
            deleteIngredients(id);
            deleteDiets(id);
            var recipe = ds.Recipes.Find(id);
            ds.Recipes.Remove(recipe);
            ds.SaveChanges();
        }

        public IEnumerable<IngredientBaseViewModel> IngredientGetAll()
        {
            return mapper.Map<IEnumerable<INGRED>, IEnumerable<IngredientBaseViewModel>>(ds.Ingreds);
        }

        public SearchViewModel searchByTitle(SearchViewModel search){
            var items = ds.Recipes.Where(e => e.title.Contains(search.searchString));
            var listItems = items.ToList();
            search.recipeList = mapper.Map<List<RECIPE>, List<RecipeWithMatchedIngred>>(listItems);
            return search;
        }

        public SearchViewModel searchForRecipe(SearchViewModel search)
        {
            List<INGRED> ingreds = new List<INGRED>();
            //split ingred
            List<String> selectedIngreds = search.searchString.Split(',').ToList();
            //search for ingreds 
            List<RECIPE_INGREDS> recipesIngreds = new List<RECIPE_INGREDS>();
            List<RECIPE> recipes = new List<RECIPE>();
            Dictionary<int, int> matchedIngredients = new Dictionary<int, int>();
            foreach (var item in selectedIngreds) {
                IEnumerable<INGRED> ingredSearch = ds.Ingreds.Where(e => e.ingred_Name.Contains(item));
                ingreds.AddRange(ingredSearch);
            }
            
            foreach (var item in ingreds)
            {
                IEnumerable<RECIPE_INGREDS> bridge = ds.Recipe_Ingreds.SqlQuery("Select * from RECIPE_INGREDS where ingred_Id = " + item.ingred_ID);
                recipesIngreds.AddRange(bridge);
                
            }

            foreach (var tmp in recipesIngreds)
            {
                int value;
                if (matchedIngredients.TryGetValue(tmp.recipe_ID, out value))
                {
                    matchedIngredients[tmp.recipe_ID] = value + 1;
                }
                else
                {
                    matchedIngredients.Add(tmp.recipe_ID, 1);
                }

            }
            foreach (var item in recipesIngreds)
            {
                IEnumerable<RECIPE> derp = ds.Recipes.Where(e => e.recipe_ID == item.recipe_ID);
                recipes.AddRange(derp);
            }

            recipes = recipes.Distinct().ToList();
            
            var matchedIngredRecipes = mapper.Map<List<RECIPE>, List<RecipeWithMatchedIngred>>(recipes);

            foreach(var item in matchedIngredRecipes)
            {
                int value;
                if (matchedIngredients.TryGetValue(item.recipe_Id, out value))
                {
                    item.matchedIngredients = value;
                }
            }
            search.recipeList = matchedIngredRecipes;
            return search;
        }

        public void addIngredientsForRecipes(int id, String[] selectedIds)
        {
            for (int i = 0; i < selectedIds.Length; i++)
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
            foreach (var item in ingreds)
            {
                selectedIngreds.Add(item.ingred_ID.ToString());
            }
            return selectedIngreds;
        }

        public IEnumerable<IngredientBaseViewModel> ingredsForRecipeViewModel(int? id)
        {
            List<int> selectedIngreds = new List<int>();
            IEnumerable<RECIPE_INGREDS> ingreds = ds.Recipe_Ingreds.SqlQuery("Select * from RECIPE_INGREDS where recipe_Id = " + id);
            foreach (var item in ingreds)
            {
                selectedIngreds.Add(item.ingred_ID);
            }

            List<INGRED> baseIngreds = new List<INGRED>();
            foreach (var item in selectedIngreds)
            {
                baseIngreds.Add(ds.Ingreds.SingleOrDefault(e => e.ingred_ID == item));
            }
            return mapper.Map<IEnumerable<INGRED>, IEnumerable<IngredientBaseViewModel>>(baseIngreds);
        }

        public void deleteIngredients(int id)
        {
            ds.Database.ExecuteSqlCommand("delete from RECIPE_INGREDS where recipe_Id = " + id);
            ds.SaveChanges();
        }

        public void deleteDiets(int id)
        {
            ds.Database.ExecuteSqlCommand("delete from DIET_RECIPES where recipe_Id = " + id);
            ds.SaveChanges();
        }

        public void DeleteDietsForUser(int id)
        {
            ds.Database.ExecuteSqlCommand("delete from USER_DIETS where user_Id = " + id);
            ds.SaveChanges();
        }

        public void DeleteAllergiesForUser(int id)
        {
            ds.Database.ExecuteSqlCommand("delete from USER_ALLERGIES where user_Id = " + id);
            ds.SaveChanges();
        }

        public void addDietsForRecipes(int id, String[] selectedIds)
        {
            for (int i = 0; i < selectedIds.Length; i++)
            {
                String query = "INSERT INTO DIET_RECIPES (recipe_ID, diet_ID) VALUES (" + id + "," + Int32.Parse(selectedIds[i]) + ")";
                ds.Database.ExecuteSqlCommand(query);
            }
            ds.SaveChanges();
        }

        public void UpdateDietsForUser(int id, String[] selectedIds)
        {
            for (int i = 0; i < selectedIds.Length; i++)
            {
                String query = "INSERT INTO USER_DIETS (user_ID, diet_ID) VALUES (" + id + "," + Int32.Parse(selectedIds[i]) + ")";
                ds.Database.ExecuteSqlCommand(query);
            }
            ds.SaveChanges();
        }

        public void UpdateAllergiesForUser(int id, String[] selectedIds)
        {
            for (int i = 0; i < selectedIds.Length; i++)
            {
                String query = "INSERT INTO USER_ALLERGIES (user_ID, allergy_ID) VALUES (" + id + "," + Int32.Parse(selectedIds[i]) + ")";
                ds.Database.ExecuteSqlCommand(query);
            }
            ds.SaveChanges();
        }

        public List<String> dietsForRecipe(int? id)
        {
            List<String> selectedDiets = new List<string>();
            IEnumerable<DIET_RECIPES> diets = ds.Diet_Recipes.SqlQuery("Select * from DIET_RECIPES where recipe_Id = " + id);
            foreach (var item in diets)
            {
                selectedDiets.Add(item.diet_ID.ToString());
            }
            return selectedDiets;
        }

        public IEnumerable<DietDescViewModel> DietsForChangeScreen()
        {
            List<String> selectedDiets = new List<string>();
            IEnumerable<DIET> diets = ds.Diets.SqlQuery("Select * from DIETS where diet_Id!=10");
            return mapper.Map<IEnumerable<DIET>, IEnumerable<DietDescViewModel>>(diets);
        }

        public List<String> DietsForUser(int? id)
        {
            List<String> selectedDiets = new List<string>();
            IEnumerable<USER_DIETS> diets = ds.User_Diets.SqlQuery("Select * from USER_DIETS where user_Id = " + id);
            foreach (var item in diets)
            {
                selectedDiets.Add(item.diet_Id.ToString());
            }
            return selectedDiets;
        }

        public List<String> AllergiesForUser(int? id)
        {
            List<String> selectedAllergies = new List<string>();
            IEnumerable<USER_ALLERGIES> allergies = ds.User_Allergies.SqlQuery("Select * from USER_ALLERGIES where user_Id = " + id);
            foreach (var item in allergies)
            {
                selectedAllergies.Add(item.allergy_Id.ToString());
            }
            return selectedAllergies;
        }

        public IEnumerable<DietDescViewModel> DietsForUserProfile(int? id)
        {
            List<int> selectedDiets = new List<int>();
            IEnumerable<USER_DIETS> diets = ds.User_Diets.SqlQuery("Select * from USER_DIETS where user_Id = " + id);
            foreach (var item in diets)
            {
                selectedDiets.Add(item.diet_Id);
            }

            List<DIET> baseDiets = new List<DIET>();
            foreach (var item in selectedDiets)
            {
                baseDiets.Add(ds.Diets.SingleOrDefault(e => e.diet_ID == item));
            }
            return mapper.Map<IEnumerable<DIET>, IEnumerable<DietDescViewModel>>(baseDiets);
        }

        public IEnumerable<AllergyViewModel> AllergiesForUserProfile(int? id)
        {
            List<int> selectedAllergies = new List<int>();
            IEnumerable<USER_ALLERGIES> allergies = ds.User_Allergies.SqlQuery("Select * from USER_ALLERGIES where user_Id = " + id);
            foreach (var item in allergies)
            {
                selectedAllergies.Add(item.allergy_Id);
            }

            List<ALLERGY> baseAllergies = new List<ALLERGY>();
            foreach (var item in selectedAllergies)
            {
                baseAllergies.Add(ds.Allergies.SingleOrDefault(e => e.allergy_ID == item));
            }
            return mapper.Map<IEnumerable<ALLERGY>, IEnumerable<AllergyViewModel>>(baseAllergies);
        }

        public IEnumerable<DietDescViewModel> dietsForRecipeViewModel(int? id)
        {
            List<int> selectedDiets = new List<int>();
            IEnumerable<DIET_RECIPES> diets = ds.Diet_Recipes.SqlQuery("Select * from DIET_RECIPES where recipe_Id = " + id);
            foreach (var item in diets)
            {
                selectedDiets.Add(item.diet_ID);
            }

            List<DIET> baseDiets = new List<DIET>();
            foreach (var item in selectedDiets)
            {
                baseDiets.Add(ds.Diets.SingleOrDefault(e => e.diet_ID == item));
            }
            return mapper.Map<IEnumerable<DIET>, IEnumerable<DietDescViewModel>>(baseDiets);
        }


        public IEnumerable<UserBaseViewModel> UserFindAll()
        {
            var users = ds.Users.Where(e => String.IsNullOrEmpty(e.admin_ID.ToString()));
            return mapper.Map<IEnumerable<USER>, IEnumerable<UserBaseViewModel>>(users);
        }

        public bool CanUserEdit(int recipeID) {
            var username = HttpContext.Current.User.Identity.Name;
            var users = ds.Users.SingleOrDefault(e => e.userName == username);
            if (String.IsNullOrEmpty(users.admin_ID.ToString()))
            {
                var recipe = ds.Recipes.SingleOrDefault(e => e.recipe_ID == recipeID);

                if (recipe == null) {
                    return false;
                }

                if (recipe.author == username)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public bool IsUserAdmin(string id)
        {
            var users = ds.Users.SingleOrDefault(e => e.userName == id);
            if (String.IsNullOrEmpty(users.admin_ID.ToString()))
            {
                return false;
            }
            else {
                return true;
            }

        }

        public string GetCurrentUsername()
        {
            var username = HttpContext.Current.User.Identity.Name;

            var user = ds.Users.SingleOrDefault(u => u.userName == username);

            return user.userName;
        }

        public string GetCurrentUserEmail()
        {
            var username = HttpContext.Current.User.Identity.Name;

            var user = ds.Users.SingleOrDefault(u => u.userName == username);

            return user.userEmail;
        }

        public UserBaseViewModel GetUserById(int? id)
        {
            //Find user from their unique ID number
            var user = ds.Users.SingleOrDefault(e => e.user_ID == id);

            //Reutn null if no match found
            return user == null ? null : mapper.Map<USER, UserBaseViewModel>(user);
        }

        public RecoverViewModel GetUserByEmail(string email)
        {
            //Find user from their unique ID number
            var user = ds.Users.SingleOrDefault(e => e.userEmail == email);

            //Reutn null if no match found
            return user == null ? null : mapper.Map<USER, RecoverViewModel>(user);
        }

        public IEnumerable<UserBaseViewModel> UserFind(UserFindViewModel find)
        {
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

        public bool ContactAdmin(ContactUsViewModel contactUs)
        {
            try
            {
                string adminEmail = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"].ToString();
                string adminPassword = System.Configuration.ConfigurationManager.AppSettings["AdminPassword"].ToString();
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(adminEmail, adminPassword);

                MailMessage mailMessage = new MailMessage(adminEmail, adminEmail, contactUs.emailAddress, contactUs.feedBack);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                client.Send(mailMessage);

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool RecoverInfo(RecoverViewModel recovery)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                //client.Credentials = new NetworkCredential(adminEmail, adminPassword);

                //MailMessage mailMessage = new MailMessage(adminEmail, adminEmail, contactUs.emailAddress, contactUs.feedBack);
                //mailMessage.IsBodyHtml = true;
                //mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                //client.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        private List<String> HashPasswordRegister(string password)
        {
            List<String> passwordAndSalt = new List<string>();
            RNGCryptoServiceProvider salt = new RNGCryptoServiceProvider();
            byte[] saltBytes = new byte[20];
            salt.GetBytes(saltBytes);
            var sal = Convert.ToBase64String(saltBytes);
            var hasPassword = password + sal;
            HashAlgorithm hashAlg = new SHA256CryptoServiceProvider();
            byte[] hashBytes = Encoding.UTF8.GetBytes(hasPassword);
            byte[] hashed = hashAlg.ComputeHash(hashBytes);
            passwordAndSalt.Add(Convert.ToBase64String(saltBytes));
            passwordAndSalt.Add(Convert.ToBase64String(hashed));
            return passwordAndSalt;
        }
        private String HashPasswordLogin(string password, string salt)
        {
            var hashedPassword = password + salt;
            HashAlgorithm hashAlg = new SHA256CryptoServiceProvider();
            byte[] hashBytes = Encoding.UTF8.GetBytes(hashedPassword);
            byte[] hashed = hashAlg.ComputeHash(hashBytes);
            return Convert.ToBase64String(hashed);
        }
        public bool LoginUser(LoginViewModel loginModel)
        {
            var loggedInUserName = ds.Users.Where(x => x.userName == loginModel.userEmail).FirstOrDefault();
            if(loggedInUserName != null)
            {
                if(loggedInUserName.salt.Length < 4)
                {
                    if(loggedInUserName.password == loginModel.password)
                    {
                        List<String> hash = HashPasswordRegister(loginModel.password);
                        String query = "UPDATE USERS SET password='" + hash.ElementAt(1) + "', salt='" + hash.ElementAt(0) + "' WHERE userName = '" + loggedInUserName.userName + "'";
                        ds.Database.ExecuteSqlCommand(query);
                        ds.SaveChanges();
                        FormsAuthentication.SetAuthCookie(loggedInUserName.userName, false);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                var hashedPassword = HashPasswordLogin(loginModel.password, loggedInUserName.salt);
                if(loggedInUserName.password == hashedPassword)
                {
                    if (loggedInUserName.banUser == true)
                    {
                        return true;
                    }
                    FormsAuthentication.SetAuthCookie(loggedInUserName.userName, false);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            var loggedInEmail = ds.Users.Where(x => x.userEmail == loginModel.userEmail).FirstOrDefault();
            if (loggedInEmail != null)
            {
                if (loggedInEmail.salt == null)
                {
                    if (loggedInEmail.password == loginModel.password)
                    {
                        List<String> hash = HashPasswordRegister(loggedInEmail.password);
                        String query = "UPDATE USERS SET password='" + hash.ElementAt(1) + "', salt='" + hash.ElementAt(0) + "' WHERE userEmail = '" + loggedInEmail.userEmail + "'";
                        ds.Database.ExecuteSqlCommand(query);
                        ds.SaveChanges();
                        FormsAuthentication.SetAuthCookie(loggedInEmail.userName, false);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                var hashedPassword = HashPasswordLogin(loginModel.password, loggedInEmail.salt);
                if (loggedInEmail.password == hashedPassword)
                {
                    if (loggedInEmail.banUser == true)
                    {
                        return true;
                    }
                    FormsAuthentication.SetAuthCookie(loggedInEmail.userName, false);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        public bool RegisterUser(RegisterViewModel registerModel)
        {
            //no duplicate email
            var loggedInUserEmail = ds.Users.Where(x => x.userEmail == registerModel.userEmail).Count();

            var loggedInUserName = ds.Users.Where(x => x.userName == registerModel.userName).Count();

            if (loggedInUserEmail > 0)
            {
                return true;
            }

            if (loggedInUserName > 0)
            {
                return true;
            }

            //alphanumeric
            Regex r = new Regex("^[a-zA-Z0-9_]*$");
            if (!r.IsMatch(registerModel.userName))
            {
                return true;
            }

            if (!r.IsMatch(registerModel.password))
            {
                return true;
            }

            if (registerModel.password != registerModel.confirmPassword)
            {
                return true;
            }

            List<String> hashPassword = HashPasswordRegister(registerModel.password);
            registerModel.acceptWaiver = false;
            registerModel.banUser = false;
            registerModel.email_Verified = false;
            registerModel.salt = "AA";
            registerModel.GUID = Guid.NewGuid().ToString();
            var addedItem = ds.Users.Add(mapper.Map<RegisterViewModel, USER>(registerModel));
            ds.SaveChanges();
            if (addedItem != null)
            {
                String query = "UPDATE USERS SET password='" + hashPassword.ElementAt(1) + "', salt='" + hashPassword.ElementAt(0) +"' WHERE userEmail = '" + addedItem.userEmail + "'";
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
                FormsAuthentication.SetAuthCookie(addedItem.userName, false);
                bool verifyEmailSent = SendEmailVerification(registerModel.GUID, registerModel.userEmail);
                if (verifyEmailSent)
                {
                    return false;
                }
            }

            return true;
        }

        public bool RecoverUser(RecoverViewModel recoverModel)
        {
            //Check if email exists
            var loggedInUserEmail = ds.Users.Where(x => x.userEmail == recoverModel.userEmail).Count();

            if (loggedInUserEmail == 0)
            {
                return true;
            }

            //Check if user was banned
            if (recoverModel.banUser)
            {
                return true;
            }

            //Check if email address was not verified
            if (!recoverModel.email_Verified)
            {
                return true;
            }

            recoverModel.GUID = Guid.NewGuid().ToString();

            try
            {
                String query = "UPDATE USERS SET GUID = \"" + recoverModel.GUID + "\" WHERE userEmail = '" + recoverModel.userEmail + "'";
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
            }
            catch
            {
                return true;
            }


            bool verifyEmailSent = SendPasswordRecovery(recoverModel.GUID, recoverModel.userEmail, recoverModel.userName);
            if (verifyEmailSent)
            {
                return false;
            }

            return true;
        }

        public void AccountVerification(string id)
        {
            String query = "UPDATE USERS SET email_Verified = 1 WHERE GUID = '" + id + "'";
            ds.Database.ExecuteSqlCommand(query);
        }

        public bool logoutUser()
        {
            try
            {
                FormsAuthentication.SignOut();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool SendEmailVerification(string GUID, string emailID)
        {
            var verificationURL = "/Home/VerifyAccount/" + GUID;
            var link = "http://localhost:5657" + verificationURL;
            try
            {
                string adminEmail = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"].ToString();
                string adminPassword = System.Configuration.ConfigurationManager.AppSettings["AdminPassword"].ToString();
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(adminEmail, adminPassword);

                String Subject = "Your account has been successfully created";
                String Body = "<br/><br/> Thanks for joining Cooking Curator. Your account has been created successfully."
                            + " Please click on the link to verify your account <a href='" + link + "'>" + link + "</a>";
                MailMessage mailMessage = new MailMessage(adminEmail, emailID, Subject, Body);
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool SendPasswordRecovery(string GUID, string emailID, string userName)
        {
            var verificationURL = "/Home/Reset/" + GUID;
            var link = "http://localhost:5657" + verificationURL;
            try
            {
                string adminEmail = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"].ToString();
                string adminPassword = System.Configuration.ConfigurationManager.AppSettings["AdminPassword"].ToString();
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(adminEmail, adminPassword);

                String Subject = "Forgotten Password - Cooking Curator";
                String Body = "<br/><br/> A password recovery was requested for this email address for the account " + userName + " ."
                            + "<br/> Please click on this link to to reset your password for Cooking Curator <a href='" + link + "'>" + link + "</a>"
                            + "<br/> If you did not request this, ignore and delete this email.";
                MailMessage mailMessage = new MailMessage(adminEmail, emailID, Subject, Body);
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        public int FetchUserId(String userNameOrEmail)
        {
            var loggedInUserName = ds.Users.Where(x => x.userName == userNameOrEmail || x.userEmail == userNameOrEmail).FirstOrDefault();

            return loggedInUserName.user_ID;
        }

        public IEnumerable<RecipeSourceViewModel> RecipeSourceGetAll()
        {
            // The ds object is the data store
            // It has a collection for each entity it manages

            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeSourceViewModel>>(ds.Recipes.Where(t => t.source_ID.HasValue == true));
        }

        public IEnumerable<IngredientBaseViewModel> IngredGetAll()
        {
            return mapper.Map<IEnumerable<INGRED>, IEnumerable<IngredientBaseViewModel>>(ds.Ingreds);
        }

        public bool IsWaiverAccepted(int Id)
        {
            var user = ds.Users.SingleOrDefault(e => e.user_ID == Id);
            if (user.acceptWaiver)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool waiverAccepted()
        {
            var username = GetCurrentUsername();
            var user = ds.Users.Where(u => u.userName == username).FirstOrDefault();
            if (user.acceptWaiver)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetCurrentUserId()
        {
            var username = GetCurrentUsername();
            var user = ds.Users.Where(u => u.userName == username).FirstOrDefault();
            return user.user_ID;
        }
        public bool AcceptWaiverByUser(UserAcceptWaiverViewModel user)
        {
            try
            {
                String query = "UPDATE USERS SET acceptWaiver = 1 WHERE user_ID = " + user.user_ID;
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool ChangePW(RecoverViewModel resetPW)
        {
            var user = ds.Users.Where(u => u.GUID == resetPW.GUID).FirstOrDefault();
            var hashedPw = HashPasswordLogin(resetPW.password, user.salt);
            try
            {
                String query = "UPDATE USERS SET password = \"" + hashedPw + "\" WHERE GUID = \"" + resetPW.GUID + "\"";
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool CheckForVote(int recipeId)
        {
            var username = HttpContext.Current.User.Identity.Name;

            var votingUser = ds.Users.SingleOrDefault(u => u.userName == username);
            var checkVote = ds.Recipe_Users.SingleOrDefault(v => v.recipe_ID == recipeId && v.user_ID == votingUser.user_ID);

            if (checkVote == null)
            {
                return false;
            }
            else if(checkVote.voting != 1 && checkVote.voting != -1)
            {
               return false;
            }
            else
            {
                return true;
            }
        }

        public void AlterRating(int recipeId, int ratingChange)
        {
            String query;
            var username = HttpContext.Current.User.Identity.Name;
            var votingUser = ds.Users.SingleOrDefault(u => u.userName == username);
            var checkVote = ds.Recipe_Users.SingleOrDefault(v => v.recipe_ID == recipeId && v.user_ID == votingUser.user_ID);

            if (checkVote == null)
            {
                query = "INSERT INTO RECIPE_USERS VALUES (" + recipeId + ", " + votingUser.user_ID + ", " + ratingChange + ", " + 0 + ", " + 0 + ")";
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
            }
            else
            {
                query = "UPDATE RECIPE_USERS SET voting = " + ratingChange + " WHERE recipe_ID = " + checkVote.recipe_ID + " AND user_ID = " + votingUser.user_ID;
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
            }

            query = "SELECT SUM(voting) FROM RECIPE_USERS WHERE recipe_ID = " + recipeId;
            int sum = ds.Database.SqlQuery<int>(query).FirstOrDefault();

            //Update recipe's overall rating
            query = "UPDATE RECIPES SET rating = " + sum + " WHERE recipe_ID = " + recipeId;
            ds.Database.ExecuteSqlCommand(query);
            ds.SaveChanges();
        }
          
        public IEnumerable<RecipeWithImagesViewModel> FilterRecipesByCountry(string countryName)
        {
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.Where(r => r.country.Contains(countryName)));
        }

        public IEnumerable<RecipeWithImagesViewModel> FilterRecipesByMealType(string mealType)
        {
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.Where(r => r.mealTimeType.Contains(mealType)));
        }

        public IEnumerable<RecipeWithImagesViewModel> FilterRecipesByMealTypeAndCountry(string mealType, string countryName)
        {
            var recipes = ds.Recipes.Where(r => r.country.Contains(countryName));
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(recipes.Where(r => r.mealTimeType.Contains(mealType)));
        }

        public IEnumerable<RecipeWithImagesViewModel> FilterVerifiedRecipes(string verified, IEnumerable<RecipeWithImagesViewModel> recipes)
        {
            if(recipes == null)
            {
                if (verified.Equals("1"))
                {
                    return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.Where(r => r.verified == true));
                }
                else
                {
                    return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.Where(r => r.verified == false));
                }
            }
            else
            {
                if (verified.Equals("1"))
                {
                    return recipes.Where(r => r.verified == true);
                }
                else
                {
                    return recipes.Where(r => r.verified == false);
                }
            }
        }

        public int BookMarkRecipe(int id)
        {
            var userName = HttpContext.Current.User.Identity.Name;
            var user = ds.Users.Where(u => u.userName == userName).FirstOrDefault();
            var bookmark = ds.Recipe_Users.Where(b => b.user_ID == user.user_ID && b.recipe_ID == id).FirstOrDefault();
            if(bookmark != null)
            {
                if (bookmark.bookmarked.GetValueOrDefault())
                {
                    return 1;
                }
                else
                {
                    String query = "UPDATE RECIPE_USERS SET bookmarked=1 WHERE recipe_ID = " + id + " && user_ID = " + bookmark.user_ID;
                    ds.Database.ExecuteSqlCommand(query);
                    ds.SaveChanges();
                    return 0;
                }
            }
            else 
            {
                String query = "INSERT INTO RECIPE_USERS(recipe_ID, user_ID, voting, reported, bookmarked) VALUES (" + id + ", " + user.user_ID + ", " + "0, 0, 1)";
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
                return 0;
            }
        }

        public IEnumerable<BookmarkViewModel> GetAllBookmarks()
        {
            var userName = HttpContext.Current.User.Identity.Name;
            var user = ds.Users.Where(u => u.userName == userName).FirstOrDefault();
            var bookmarks = ds.Recipe_Users.Where(b => b.user_ID == user.user_ID && b.bookmarked == true);
            var bmks = mapper.Map<IEnumerable<RECIPE_USERS>, IEnumerable<BookmarkViewModel>>(bookmarks);
            foreach (var bmk in bmks)
            {
                bmk.Recipe = mapper.Map<RECIPE, RecipeBaseViewModel>(ds.Recipes.Find(bmk.recipe_ID));
            }

            return bmks;
        }

        public bool DeleteBookmark(int id)
        {
            var userName = HttpContext.Current.User.Identity.Name;
            var user = ds.Users.Where(u => u.userName == userName).FirstOrDefault();
            var bookmark = ds.Recipe_Users.Where(b => b.user_ID == user.user_ID && b.recipe_ID == id).FirstOrDefault();
            try
            {
                String query = "UPDATE RECIPE_USERS SET bookmarked=0 WHERE user_ID = " + bookmark.user_ID + "&& recipe_ID = " + bookmark.recipe_ID;
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
                return false ;
            }
            catch{
                return true;
            }

        }

        //Retreive all allergy descriptions
        public IEnumerable<AllergyViewModel> AllergyGetAll()
        {
            return mapper.Map<IEnumerable<ALLERGY>, IEnumerable<AllergyViewModel>>(ds.Allergies);
        }

        //Retrieve all ingredients per allergy with unique names
        public IEnumerable<IngredBase> GetIngredByAllergen(string allergy_Name)
        {
            var allergy_num = ds.Allergies.SingleOrDefault(a => a.allergyName == allergy_Name);

            IEnumerable<INGRED> ingredients = ds.Ingreds.SqlQuery("Select * FROM INGRED WHERE ingred_ID IN (SELECT ingred_ID FROM ALLERGY_INGREDS WHERE allergy_ID = " + allergy_num.allergy_ID + ")");

            return ingredients == null ? null : mapper.Map<IEnumerable<INGRED>, IEnumerable<IngredBase>>(ingredients);
        }

        public IEnumerable<RecipeWithImagesViewModel> SortRecipes(string sortOrder, IEnumerable<RecipeWithImagesViewModel> recipes)
        {
            IEnumerable<RecipeWithImagesViewModel> Sortedrecipes;
            if(recipes == null)
            {
                switch (sortOrder)
                {
                    case "title_desc":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderByDescending(r => r.title));
                        break;
                    case "ratings_desc":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderByDescending(r => r.rating));
                        break;
                    case "ratings":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderBy(r => r.rating));
                        break;
                    case "author_desc":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderByDescending(r => r.author));
                        break;
                    case "author":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderBy(r => r.author));
                        break;
                    case "sourceId_desc":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderByDescending(r => r.source_ID));
                        break;
                    case "sourceId":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderBy(r => r.source_ID));
                        break;
                    case "country_desc":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderByDescending(r => r.country));
                        break;
                    case "country":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderBy(r => r.country));
                        break;
                    case "mealTimeType_desc":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderByDescending(r => r.mealTimeType));
                        break;
                    case "mealTimeType":
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderBy(r => r.mealTimeType));
                        break;
                    default:
                        Sortedrecipes = mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeWithImagesViewModel>>(ds.Recipes.OrderBy(r => r.title));
                        break;
                }
            }
            else
            {
                switch (sortOrder)
                {
                    case "title_desc":
                        Sortedrecipes = recipes.OrderByDescending(r => r.title);
                        break;
                    case "ratings_desc":
                        Sortedrecipes = recipes.OrderByDescending(r => r.rating);
                        break;
                    case "ratings":
                        Sortedrecipes = recipes.OrderBy(r => r.rating);
                        break;
                    case "author_desc":
                        Sortedrecipes = recipes.OrderByDescending(r => r.author);
                        break;
                    case "author":
                        Sortedrecipes = recipes.OrderBy(r => r.author);
                        break;
                    case "sourceId_desc":
                        Sortedrecipes = recipes.OrderByDescending(r => r.source_ID);
                        break;
                    case "sourceId":
                        Sortedrecipes = recipes.OrderBy(r => r.source_ID);
                        break;
                    case "country_desc":
                        Sortedrecipes = recipes.OrderByDescending(r => r.country);
                        break;
                    case "country":
                        Sortedrecipes = recipes.OrderBy(r => r.country);
                        break;
                    case "mealTimeType_desc":
                        Sortedrecipes = recipes.OrderByDescending(r => r.mealTimeType);
                        break;
                    case "mealTimeType":
                        Sortedrecipes = recipes.OrderBy(r => r.mealTimeType);
                        break;
                    default:
                        Sortedrecipes = recipes.OrderBy(r => r.title);
                        break;
                }
            }
            
            return Sortedrecipes;
        }
      
        public int ReportRecipe(ReportRecipeViewModel reportedRecipe)
        {
            var username = GetCurrentUsername();
            reportedRecipe.userName = username;
            var user = ds.Users.Where(u => u.userName == username).FirstOrDefault();
            var reportRecipe = ds.Recipe_Users.Where(b => b.user_ID == user.user_ID && b.recipe_ID == reportedRecipe.recipeId).FirstOrDefault();
            if (reportRecipe != null)
            {
                if (reportRecipe.reported.GetValueOrDefault())
                {
                    return 1;
                }
                else
                {
                    bool error = SendReportRecipeEmail(reportedRecipe);
                    if (error)
                    {
                        String query = "UPDATE RECIPE_USERS SET reported=1 WHERE recipe_ID = " + reportedRecipe.recipeId + " && user_ID = " + user.user_ID;
                        ds.Database.ExecuteSqlCommand(query);
                        ds.SaveChanges();
                        return 0;
                    }
                    else
                    {
                        return 2;
                    }
                }
            }
            else
            {
                bool error = SendReportRecipeEmail(reportedRecipe);
                if (error)
                {
                    String query = "INSERT INTO RECIPE_USERS(recipe_ID, user_ID, voting, reported, bookmarked) VALUES (" + reportedRecipe.recipeId + ", " + user.user_ID + ", " + "0, 1, 0)";
                    ds.Database.ExecuteSqlCommand(query);
                    ds.SaveChanges();
                    return 0;
                }
                else
                {
                    return 2;
                }
            }
        }

        private bool SendReportRecipeEmail(ReportRecipeViewModel reportedRecipe)
        {
            var URL = "/Recipe/Details/" + reportedRecipe.recipeId;
            var link = "http://localhost:5657" + URL;
            try
            {
                string adminEmail = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"].ToString();
                string adminPassword = System.Configuration.ConfigurationManager.AppSettings["AdminPassword"].ToString();
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(adminEmail, adminPassword);

                String Subject = "Report Recipe - Cooking Curator";
                String Body = "<br/><br/>A recipe was reported by user " + reportedRecipe.userName + " ."
                    + "<br/> Due to the reason mentioned below :-"
                    + "<br/><br/> " + reportedRecipe.feedBack
                + "<br/><br/> Please click on this link to to view the recipe details <a href='" + link + "'>" + link + "</a>";
                            
                MailMessage mailMessage = new MailMessage(adminEmail, adminEmail, Subject, Body);
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
