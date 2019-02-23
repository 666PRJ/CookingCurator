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

                cfg.CreateMap<RecipeAddViewForm, RECIPE>();

                cfg.CreateMap<RecipeIngred, RECIPE>();

                cfg.CreateMap<UserFindViewModel, USER>();

                cfg.CreateMap<INGRED, IngredientBaseViewModel>();

                cfg.CreateMap<RegisterViewModel, USER>();

                cfg.CreateMap<UserBaseViewModel, UserAcceptWaiverViewModel>();

                cfg.CreateMap<UserAcceptWaiverViewModel, USER>();

                cfg.CreateMap<USER, ChangeUsernameViewModel>();

                cfg.CreateMap<USER, ChangePasswordViewModel>();

                cfg.CreateMap<USER, RecoverViewModel>();
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

        public bool ChangeUsername(ChangeUsernameViewModel newUsername) {
            //var user = ds.Users.SingleOrDefault(e => e.user_ID == newUsername.user_ID);
            var user = ds.Users.SingleOrDefault(e => e.userName == HttpContext.Current.User.Identity.Name);
            var duplicateFound = ds.Users.Where(f => f.userName == newUsername.userName);
            if (user == null) {
                return false;
            }
            if (duplicateFound.Count() > 0) {
                return false;
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

            if (user.password != newPassword.OldPassword)
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

            user.password = newPassword.password;
            ds.Entry(user).State = System.Data.Entity.EntityState.Modified;

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


        public RecipeBaseViewModel RecipeAdd(RecipeAddViewForm recipe)
        {
            // Attempt to add the new item.
            // Notice how we map the incoming data to the Customer design model class.
            var addedItem = ds.Recipes.Add(mapper.Map<RecipeAddViewForm, RECIPE>(recipe));
            deleteIngredients(addedItem.recipe_ID);
            addIngredientsForRecipes(addedItem.recipe_ID, recipe.selectedIngredsId);
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
            // Attempt to add the new item.
            // Notice how we map the incoming data to the Customer design model class.
            var addedItem = ds.Recipes.Add(mapper.Map<RecipeVerifiedAddViewModel, RECIPE>(recipe));
            deleteIngredients(addedItem.recipe_ID);
            addIngredientsForRecipes(addedItem.recipe_ID, recipe.selectedIngredsId);
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
            addIngredientsForRecipes(recipeIng.recipe.recipe_Id, recipeIng.selectedIngredsId);
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

        public SearchViewModel searchForRecipe(SearchViewModel search)
        {
            List<INGRED> ingreds = new List<INGRED>();
            //split ingred
            List<String> selectedIngreds = search.searchString.Split(',').ToList();
            //search for ingreds 
            List<RECIPE_INGREDS> recipesIngreds = new List<RECIPE_INGREDS>();
            List<RECIPE> recipes = new List<RECIPE>();
            foreach (var item in selectedIngreds) {
                IEnumerable<INGRED> ingredSearch = ds.Ingreds.Where(e => e.ingred_Name.Contains(item));
                ingreds.AddRange(ingredSearch);
            }

            foreach (var item in ingreds)
            {
                IEnumerable<RECIPE_INGREDS> bridge = ds.Recipe_Ingreds.SqlQuery("Select * from RECIPE_INGREDS where ingred_Id = " + item.ingred_ID);
                recipesIngreds.AddRange(bridge);
            }

            foreach (var item in recipesIngreds)
            {
                IEnumerable<RECIPE> derp = ds.Recipes.Where(e => e.recipe_ID == item.recipe_ID);
                recipes.AddRange(derp);
            }

            recipes = recipes.Distinct().ToList();

            search.recipeList = mapper.Map<List<RECIPE>, List<RecipeBaseViewModel>>(recipes);

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

        public IEnumerable<UserBaseViewModel> UserFindAll()
        {
            var users = ds.Users.Where(e => String.IsNullOrEmpty(e.admin_ID.ToString()));
            return mapper.Map<IEnumerable<USER>, IEnumerable<UserBaseViewModel>>(users);
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

        public bool LoginUser(LoginViewModel loginModel)
        {
            var loggedInUserName = ds.Users.Where(x => x.userName == loginModel.userEmail && x.password == loginModel.password).FirstOrDefault();

            if (loggedInUserName != null)
            {
                if (loggedInUserName.banUser == true)
                {
                    return true;
                }
                FormsAuthentication.SetAuthCookie(loggedInUserName.userName, false);
                return false;
            }

            var loggedInEmail = ds.Users.Where(x => x.userEmail == loginModel.userEmail && x.password == loginModel.password).FirstOrDefault();
            if (loggedInEmail != null)
            {
                if (loggedInEmail.banUser == true)
                {
                    return true;
                }
                FormsAuthentication.SetAuthCookie(loggedInEmail.userName, false);
                return false;
            }

            return true;
        }

        public bool RegisterUser(RegisterViewModel registerModel)
        {
            //no duplicate email
            var loggedInUserEmail = ds.Users.Where(x => x.userEmail == registerModel.userEmail).Count();

            var loggedInUserName = ds.Users.Where(x => x.userEmail == registerModel.userEmail).Count();

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

            registerModel.acceptWaiver = false;
            registerModel.banUser = false;
            registerModel.email_Verified = false;
            registerModel.salt = "AA";
            registerModel.GUID = Guid.NewGuid().ToString();
            var addedItem = ds.Users.Add(mapper.Map<RegisterViewModel, USER>(registerModel));
            ds.SaveChanges();
            if (addedItem != null)
            {
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
                String query = "UPDATE USERS SET GUID = \"" + recoverModel.GUID + "\" WHERE userEmail = \"" + recoverModel.userEmail + "\"";
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
            var user = ds.Users.Where(a => a.GUID.Equals(id)).FirstOrDefault();
            if (user != null)
            {
                user.email_Verified = true;
                ds.SaveChanges();
            }
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
            try
            {
                String query = "UPDATE USERS SET password = \"" + resetPW.password + "\" WHERE GUID = \"" + resetPW.GUID + "\"";
                ds.Database.ExecuteSqlCommand(query);
                ds.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public IEnumerable<RecipeBaseViewModel> FilterRecipesByCountry(string countryName)
        {
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(ds.Recipes.Where(r => r.country.Contains(countryName)));
        }

        public IEnumerable<RecipeBaseViewModel> FilterRecipesByMealType(string mealType)
        {
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(ds.Recipes.Where(r => r.mealTimeType.Contains(mealType)));
        }

        public IEnumerable<RecipeBaseViewModel> FilterRecipesByMealTypeAndCountry(string mealType, string countryName)
        {
            var recipes = ds.Recipes.Where(r => r.country.Contains(countryName));
            return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(recipes.Where(r => r.mealTimeType.Contains(mealType)));
        }

        public IEnumerable<RecipeBaseViewModel> FilterVerifiedRecipes(string verified, IEnumerable<RecipeBaseViewModel> recipes)
        {
            if(recipes == null)
            {
                if (verified.Equals("1"))
                {
                    return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(ds.Recipes.Where(r => r.verified == true));
                }
                else
                {
                    return mapper.Map<IEnumerable<RECIPE>, IEnumerable<RecipeBaseViewModel>>(ds.Recipes.Where(r => r.verified == false));
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
    }
}