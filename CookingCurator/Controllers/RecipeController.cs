using CookingCurator.EntityModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class RecipeController : Controller
    {
        private Manager m = new Manager();

        [Authorize]
        [HttpGet]
        public ActionResult Index(string countryName, string mealType, string verified, string sortOrder, int? pageNo)
        {
            m.isUserBanned();

            if (!m.waiverAccepted())
            {
                return RedirectToAction("AcceptWaiver", "Home", new { Id = m.GetCurrentUserId().ToString(), error = "Please accept the waiver to view recipes and its related features" });
            }

            ViewBag.Username = m.GetCurrentUsername();
            int idNum = m.FetchUserId(ViewBag.Username);

            IEnumerable<DietDescViewModel> checkDiets = m.DietsForUserProfile(idNum);
            IEnumerable<AllergyViewModel> checkAllergies = m.AllergiesForUserProfile(idNum);
            IEnumerable<IngredientBaseViewModel> checkRestrictions = m.IngredsForUserProfile(idNum);

            var recipes = m.RecipeGetAllWithImages();

            if (checkDiets.Any() && checkAllergies.Any() && checkRestrictions.Any())
            {
                //All 3 used
                recipes = m.RecipeGetFilteredByAllThree(idNum);
            }
            else if(checkDiets.Any() && checkAllergies.Any() && !checkRestrictions.Any())
            {
                //Diet and Allergies, no restrictions
                recipes = m.RecipeGetFilteredByAllergyAndDiet(idNum);
            }
            else if (checkDiets.Any() && !checkAllergies.Any() && checkRestrictions.Any())
            {
                //Diets and restrictions, no allergies
                recipes = m.RecipeGetFilteredByDietandRes(idNum);
            }
            else if (!checkDiets.Any() && checkAllergies.Any() && checkRestrictions.Any())
            {
                //Allergies and restrictions, no diets
                recipes = m.RecipeGetFilteredByAllergyAndRes(idNum);
            }
            else if (checkDiets.Any() && !checkAllergies.Any() && !checkRestrictions.Any())
            {
               //Only diets selected
               recipes = m.RecipeGetFilteredByDietWithImages(idNum);
            }
            else if (!checkDiets.Any() && checkAllergies.Any() && !checkRestrictions.Any())
            {
                //Only allergies selected
                recipes = m.RecipeGetFilteredByAllergiesWithImages(idNum);
            }
            else if (!checkDiets.Any() && !checkAllergies.Any() && checkRestrictions.Any())
            {
                //Only restrictions selected
                recipes = m.RecipeGetFilteredByRestrictions(idNum);
            }

            ViewBag.Admin = m.IsUserAdmin(ViewBag.Username);

            if (!string.IsNullOrEmpty(countryName) && !string.IsNullOrEmpty(mealType))
            {
                recipes = m.FilterRecipesByMealTypeAndCountry(mealType, countryName);
            }
            else 
            {
                if (!string.IsNullOrEmpty(countryName))
                {
                     recipes = m.FilterRecipesByCountry(countryName);
                }
                else if (!string.IsNullOrEmpty(mealType))
                {
                     recipes = m.FilterRecipesByMealType(mealType);
                }
            }
            if (!string.IsNullOrEmpty(verified))
            {
                if (!verified.Equals("0"))
                {
                    recipes = m.FilterVerifiedRecipes(verified, recipes);
                }
            }
            ViewBag.titleSort = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.rateSort = sortOrder == "ratings" ? "ratings_desc" : "ratings";
            ViewBag.authorSort = sortOrder == "author" ? "author_desc" : "author";
            ViewBag.sourceIdSort = sortOrder == "sourceId" ? "sourceId_desc" : "sourceId";
            ViewBag.countrySort = sortOrder == "country" ? "country_desc" : "country";
            ViewBag.dateSort = sortOrder == "lastUpdated" ? "lastUpdated_desc" : "lastUpdated";
            ViewBag.mealTimeTypeSort = sortOrder == "mealTimeType" ? "mealTimeType_desc" : "mealTimeType";
            recipes = m.SortRecipes(sortOrder, recipes);
            if(ViewBag.Admin == false)
            {
                recipes = recipes.Where(r => (r.verified == true || r.author == ViewBag.Username));
            }
            return View(recipes.ToList().ToPagedList(pageNo ?? 1, 10));
        }

        // Get recipe details with bookmark variables (bookmark can also store vote status)
        // GET: Recipe/Details/5
        [Authorize]
        public ActionResult Details(int? id, string bookMarkError, string bookmark, string success)
        {
            if (!m.waiverAccepted())
            {
                return RedirectToAction("AcceptWaiver", "Home", new { Id = m.GetCurrentUserId().ToString(), error = "Please accept the waiver to view recipes and its related features" });
            }
            var recipe = m.RecipeWithIngredGetById(id.GetValueOrDefault());
            recipe.userID = m.UsernameToId(recipe.author);
            recipe.ingreds = m.ingredsForRecipeViewModel(id.GetValueOrDefault());
            recipe.diets = m.dietsForRecipeViewModel(id.GetValueOrDefault());
            recipe.recommended = m.giveRecommendations(m.ingredsForRecipe(id.GetValueOrDefault()), id.GetValueOrDefault());
            if (recipe.Content != null && recipe.Content_Type != null)
            {
                string base64 = Convert.ToBase64String(recipe.Content);
                recipe.fileResult = String.Format("data:{0};base64,{1}", recipe.Content_Type, base64);
            }
            ViewBag.Username = m.GetCurrentUsername();
            ViewBag.Admin = m.IsUserAdmin(ViewBag.Username);
            
            if (recipe == null)
                return HttpNotFound();
            else
            {
                if (!String.IsNullOrEmpty(bookMarkError))
                {
                    ViewBag.error = bookMarkError;
                }
                if (!String.IsNullOrEmpty(bookmark))
                {
                    ViewBag.bookMark = bookmark;
                }
                if (!String.IsNullOrEmpty(success))
                {
                    ViewBag.success = success;
                }
                return View(recipe);
            }
                
        }

        // GET: Recipe/VoteUp/5
        public ActionResult VoteUp(int? id)
        {
            RecipeBaseViewModel recipe = m.RecipeGetById(id);

            //use recipe id and logged in user's name
            if(recipe == null)
            {
                return HttpNotFound();
            }

            bool voteMade = m.CheckForVoteUp(recipe.recipe_Id);

            if (!voteMade)
            {
                m.AlterRating(recipe.recipe_Id, 1);
            }
            else
            {
                //Uses the bookmark variable to store vote status, as another function for vote variables cannot be created (avoids ambiguous function call error)
                return RedirectToAction("Details", new { id = recipe.recipe_Id, bookmark = "P" });
            }

            return RedirectToAction("Details", new { id = recipe.recipe_Id, bookmark = "Y" });
        }

        // GET: Recipe/VoteDown/5
        public ActionResult VoteDown(int? id)
        {
            RecipeBaseViewModel recipe = m.RecipeGetById(id);

            //use recipe id and logged in user's name
            if (recipe == null)
            {
                return HttpNotFound();
            }

            bool voteMade = m.CheckForVoteDown(recipe.recipe_Id);

            if (!voteMade)
            {
                m.AlterRating(recipe.recipe_Id, -1);
            }
            else
            {
                //Uses the bookmark variable to store vote status, as another function for vote variables cannot be created (avoids ambiguous function call error)
                return RedirectToAction("Details", new { id = recipe.recipe_Id, bookmark = "N" });
            }

            return RedirectToAction("Details", new { id = recipe.recipe_Id, bookmark = "Y" });
        }

        // GET: Recipe/Create
        [Authorize]
        public ActionResult Create()
        {
            if (!m.waiverAccepted())
            {
                return RedirectToAction("AcceptWaiver", "Home", new { Id = m.GetCurrentUserId().ToString(), error = "Please accept the waiver to view recipes and its related features" });
            }
            var form = new RecipeAddViewForm();

            form.ingredients = m.IngredientGetAll();
            form.selectedIngredsId = new string[0];

            form.diets = m.DietGetAll();
            form.selectedDietsId = new string[0];

            return View(form);
        }

        // GET: Recipe/Create
        [Authorize(Roles = "Admin")]
        public ActionResult CreateVerified()
        {
            var form = new RecipeVerifiedAddViewModel();

            form.ingredients = m.IngredientGetAll();
            form.selectedIngredsId = new string[0];

            form.diets = m.DietGetAll();
            form.selectedDietsId = new string[0];

            return View(form);
        }

        //CreateVerified will be only avilable to admin
        // POST: Recipe/CreateVerified
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateVerified(RecipeVerifiedAddViewModel newItem, HttpPostedFileBase file)
        {
            // Validate the input
            if (!ModelState.IsValid)
            {
                newItem.ingredients = m.IngredientGetAll();
                newItem.selectedIngredsId = new string[0];
                newItem.diets = m.DietGetAll();
                newItem.selectedDietsId = new string[0];
                return View(newItem);
            }

            if (m.IsNoSpecial(newItem.title) == false || m.IsNoSpecial(newItem.country) == false || m.IsNoSpecial(newItem.mealTimeType) == false)
            {
                ModelState.AddModelError("", "No / ; *  Allowed");
                newItem.ingredients = m.IngredientGetAll();
                newItem.selectedIngredsId = new string[0];
                newItem.diets = m.DietGetAll();
                newItem.selectedDietsId = new string[0];
                return View(newItem);
            }

            try
            {

                //Check for Diet conflict
                bool compatDiet = true;

                if (newItem.selectedDietsId != null)
                {
                    for (int i = 0; i < newItem.selectedDietsId.Length; i++)
                    {
                        //None apply cannot be selected with anything else
                        if (newItem.selectedDietsId[i] == "10" && newItem.selectedDietsId.Length > 1)
                        {
                            compatDiet = false;
                        }

                        //Vegan and Vegetarian shouldn't have meat
                        else if (newItem.selectedDietsId[i] == "8" || newItem.selectedDietsId[i] == "5")
                        {
                            for (int j = 0; j < newItem.selectedDietsId.Length; j++)
                            {
                                if (newItem.selectedDietsId[j] != "6" && newItem.selectedDietsId[j] != "5" && newItem.selectedDietsId[j] != "8")
                                {
                                    compatDiet = false;
                                }
                            }
                        }
                    }

                    if (compatDiet == false)
                    {
                        ModelState.AddModelError("", "Incompatable Diets Selected");
                        newItem.ingredients = m.IngredientGetAll();
                        newItem.selectedIngredsId = new string[0];
                        newItem.diets = m.DietGetAll();
                        newItem.selectedDietsId = new string[0];
                        return View(newItem);
                    }
                }

                // Process the input
                newItem.verified = true;
                newItem.rating = 0;
                newItem.lastUpdated = DateTime.Now;
                if(file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength / 1024 > 50)
                    {
                        var form = new RecipeVerifiedAddViewModel();
                        form.author = newItem.author;
                        form.country = newItem.country;
                        form.mealTimeType = newItem.mealTimeType;
                        form.instructions = newItem.instructions;
                        form.title = newItem.title;
                        form.ingredients = m.IngredientGetAll();
                        form.selectedIngredsId = new string[0];
                        form.diets = m.DietGetAll();
                        form.selectedDietsId = new string[0];
                        ModelState.AddModelError("", "Image size should be less than 50kb");
                        return View(form);
                    }
                    newItem.Content_Type = file.ContentType;
                    using(var reader = new System.IO.BinaryReader(file.InputStream))
                    {
                        newItem.Content = reader.ReadBytes(file.ContentLength);
                    }
                }
                var addedItem = m.RecipeVerifiedAdd(newItem);

                addedItem = m.RecipeIDUpdate(addedItem);
                // If the item was not added, return the user to the Create page
                // otherwise redirect them to the Details page.
                if (addedItem == null)
                    return View(newItem);
                else
                    return RedirectToAction("Details", new { id = addedItem.recipe_Id });
            }
            catch
            {
                newItem.ingredients = m.IngredientGetAll();
                newItem.selectedIngredsId = new string[0];
                newItem.diets = m.DietGetAll();
                newItem.selectedDietsId = new string[0];
                return View(newItem);
            }
        }

        // POST: Recipe/Create
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(RecipeAddViewForm newItem, HttpPostedFileBase file)
        {
            // Validate the input
            if (!ModelState.IsValid)
            {
                newItem.ingredients = m.IngredientGetAll();
                newItem.selectedIngredsId = new string[0];
                newItem.diets = m.DietGetAll();
                newItem.selectedDietsId = new string[0];
                return View(newItem);
            }

            if (m.IsNoSpecial(newItem.title) == false || m.IsNoSpecial(newItem.country) == false || m.IsNoSpecial(newItem.mealTimeType) == false) {
                ModelState.AddModelError("", "No / ; *  Allowed");
                newItem.ingredients = m.IngredientGetAll();
                newItem.selectedIngredsId = new string[0];
                newItem.diets = m.DietGetAll();
                newItem.selectedDietsId = new string[0];
                return View(newItem);
            }

            try
            {
                //Check for Diet conflict
                bool compatDiet = true;

                if (newItem.selectedDietsId != null)
                {
                    for (int i = 0; i < newItem.selectedDietsId.Length; i++)
                    {
                        //None apply cannot be selected with anything else
                        if (newItem.selectedDietsId[i] == "10" && newItem.selectedDietsId.Length > 1)
                        {
                            compatDiet = false;
                        }

                        //Vegan and Vegetarian shouldn't have meat
                        else if (newItem.selectedDietsId[i] == "8" || newItem.selectedDietsId[i] == "5")
                        {
                            for (int j = 0; j < newItem.selectedDietsId.Length; j++)
                            {
                                if (newItem.selectedDietsId[j] != "6" && newItem.selectedDietsId[j] != "5" && newItem.selectedDietsId[j] != "8")
                                {
                                    compatDiet = false;
                                }
                            }
                        }
                    }

                    if (compatDiet == false)
                    {
                        ModelState.AddModelError("", "Incompatable Diets Selected");
                        newItem.ingredients = m.IngredientGetAll();
                        newItem.selectedIngredsId = new string[0];
                        newItem.diets = m.DietGetAll();
                        newItem.selectedDietsId = new string[0];
                        return View(newItem);
                    }
                }

                // Process the input
                newItem.verified = false;
                newItem.rating = 0;
                newItem.lastUpdated = DateTime.Now;
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength / 1024 > 50)
                    {
                        var form = new RecipeVerifiedAddViewModel();
                        form.author = newItem.author;
                        form.country = newItem.country;
                        form.mealTimeType = newItem.mealTimeType;
                        form.instructions = newItem.instructions;
                        form.title = newItem.title;
                        form.ingredients = m.IngredientGetAll();
                        form.selectedIngredsId = new string[0];
                        form.diets = m.DietGetAll();
                        form.selectedDietsId = new string[0];
                        ModelState.AddModelError("", "Image size should be less than 50kb");
                        return View(form);
                    }
                    newItem.Content_Type = file.ContentType;
                    using (var reader = new System.IO.BinaryReader(file.InputStream))
                    {
                        newItem.Content = reader.ReadBytes(file.ContentLength);
                    }
                }
                var addedItem = m.RecipeAdd(newItem);

                addedItem = m.RecipeIDUpdate(addedItem);
                // If the item was not added, return the user to the Create page
                // otherwise redirect them to the Details page.
                if (addedItem == null) { 
                    return View(newItem);
                }
                else
                    return RedirectToAction("Details", new { id = addedItem.recipe_Id });
            }
            catch
            {
                newItem.ingredients = m.IngredientGetAll();
                newItem.selectedIngredsId = new string[0];
                newItem.diets = m.DietGetAll();
                newItem.selectedDietsId = new string[0];
                return View(newItem);
            }
        }

        // GET: Recipe/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!m.CanUserEdit(id.GetValueOrDefault())) {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Recipe_IngredViewModel recipe = m.mapper.Map<RecipeWithIngredBaseViewModel, Recipe_IngredViewModel>(m.RecipeWithIngredGetById(id));

            IEnumerable<IngredientBaseViewModel> ingredients = m.IngredientGetAll();
            String[] selectedIngreds = m.ingredsForRecipe(id).ToArray();

            IEnumerable<DietDescViewModel> diets = m.DietGetAll();
            String[] selectedDiets = m.dietsForRecipe(id).ToArray();

            if (recipe == null)
            {
                return HttpNotFound();
            }

            recipe.ingredients = ingredients;
            recipe.selectedIngredsId = selectedIngreds;
            recipe.diets = diets;
            recipe.selectedDietsId = selectedDiets;

            if (recipe.Content != null && recipe.Content_Type != null)
            {
                string base64 = Convert.ToBase64String(recipe.Content);
                recipe.fileResult = String.Format("data:{0};base64,{1}", recipe.Content_Type, base64);
            }
            return View(recipe);
        }

        // POST: Recipe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Recipe_IngredViewModel recipes, HttpPostedFileBase file)
        {
            Recipe_IngredViewModel recipe = m.mapper.Map<RecipeWithIngredBaseViewModel, Recipe_IngredViewModel>(m.RecipeWithIngredGetById(recipes.recipe_Id));

            IEnumerable<IngredientBaseViewModel> ingredients = m.IngredientGetAll();
            String[] selectedIngreds = m.ingredsForRecipe(recipes.recipe_Id).ToArray();
            IEnumerable<DietDescViewModel> diets = m.DietGetAll();
            String[] selectedDiets = m.dietsForRecipe(recipes.recipe_Id).ToArray();

            recipe.ingredients = ingredients;
            recipe.selectedIngredsId = selectedIngreds;
            recipe.diets = diets;
            recipe.selectedDietsId = selectedDiets;

            if (m.IsNoSpecial(recipes.title) == false || m.IsNoSpecial(recipes.country) == false || m.IsNoSpecial(recipes.mealTimeType) == false)
            {
                ModelState.AddModelError("", "No / ; *  Allowed");
                return View(recipe);
            }

            if (recipe == null)
            {
                return HttpNotFound();
            }

            if (recipe.Content != null && recipe.Content_Type != null)
            {
                string base64 = Convert.ToBase64String(recipe.Content);
                recipe.fileResult = String.Format("data:{0};base64,{1}", recipe.Content_Type, base64);
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Error while submitting the form. Please check the values submitted");
                return View(recipe);
            }

            try
            {

                //Check for Diet conflict
                bool compatDiet = true;

                if (recipes.selectedDietsId != null)
                {
                    for (int i = 0; i < recipes.selectedDietsId.Length; i++)
                    {
                        if (recipes.selectedDietsId[i] == "10" && recipes.selectedDietsId.Length > 1)
                        {
                            compatDiet = false;
                        }

                        else if (recipes.selectedDietsId[i] == "8" || recipes.selectedDietsId[i] == "5")
                        {
                            for (int j = 0; j < recipes.selectedDietsId.Length; j++)
                            {
                                if (recipes.selectedDietsId[j] != "6" && recipes.selectedDietsId[j] != "5" && recipes.selectedDietsId[j] != "8")
                                {
                                    compatDiet = false;
                                }
                            }
                        }
                    }

                    if (compatDiet == false)
                    {
                        ModelState.AddModelError("", "Incompatable Diets Selected");
                        return View(recipe);
                    }
                }
                if (file != null && file.ContentLength > 0)
                {
                    if(file.ContentLength / 1024 > 50)
                    {
                        ModelState.AddModelError("", "Image size should be less than 50kb");
                        return View(recipe);
                    }
                    recipes.Content_Type = file.ContentType;
                    using (var reader = new System.IO.BinaryReader(file.InputStream))
                    {
                        recipes.Content = reader.ReadBytes(file.ContentLength);
                    }
                }
                var editedrecipe = m.RecipeEdit(recipes);

                if (editedrecipe == null)
                {
                    ModelState.AddModelError("", "Error while editing a recipe. Please try again");
                    return View(recipe);
                }

                else
                {
                    return RedirectToAction("Details", new { id = editedrecipe.recipe_Id });
                }
            }
            catch
            {
                ModelState.AddModelError("", "Error while editing a recipe. Please try again");
                return View(recipe);
            }
        }

        // GET: Recipe/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeBaseViewModel recipe = m.RecipeGetById(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: Recipe/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                m.RecipeDelete(id);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error deleting recipe... Please try again");
                return View(id);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [Route("User/AuthorProfile")]
        public ActionResult Authors(string authorName)
        {
            if (!m.waiverAccepted())
            {
                return RedirectToAction("AcceptWaiver", "Home", new { Id = m.GetCurrentUserId().ToString(), error = "Please accept the waiver to view recipes and its related features" });
            }

            ViewBag.authorName = authorName;
            var r = m.RecipesByAuthor(authorName);
            return View(r);
        }

        [Authorize]
        public ActionResult Diet(string dietName)
        {
            if (dietName == null)
            {
                return View("Index");
            }

            var r = m.GetRecipesByDiet(dietName);
            ViewBag.Diet = dietName;
            return View(r);
        }

        public ActionResult BookmarkRecipe(int? ID)
        {
            if(ID == null)
            {
                return View("Details", new { id = ID});
            }
            if(m.GetAllBookmarks().Count() >= 50)
            {
                return RedirectToAction("Details", new { id = ID, bookMarkError = "You can only Bookmark 50 Recipes, please delete some bookmarks and try again." });
            }
            var error = m.BookMarkRecipe(ID.GetValueOrDefault());
            if(error == 0)
            {
                return RedirectToAction("Details", new { id = ID, bookmark = "0" });
            }
            else
            {
                return RedirectToAction("Details", new { id = ID, bookmark = "1" });
            }
        }

        [Route("/Recipe/Report/{id}")]
        public ActionResult ReportRecipe(int? id)
        {
            string username = m.GetCurrentUsername();
            if(id == null)
            {
                return View();
            }
            RecipeBaseViewModel recipe = m.RecipeGetById(id);
            ReportRecipeViewModel reportRecipe = new ReportRecipeViewModel();
            reportRecipe.recipeId = recipe.recipe_Id;
            reportRecipe.userName = username;
            reportRecipe.recipeTitle = recipe.title;

            return View(reportRecipe);
        }

        [HttpPost]
        public ActionResult ReportRecipe(ReportRecipeViewModel reportRecipe)
        {
            int error = m.ReportRecipe(reportRecipe);
            if (error == 1)
            {
                return RedirectToAction("ReportedRecipe", new { succError = error});
            }
            else if (error == 0)
            {
                return RedirectToAction("ReportedRecipe", new { succError = error });
            }
            else
            {
                ModelState.AddModelError("", "An error occurred while sending an email. Please try again!");
                return View();
            }
        }

        [Route("Recipe/Reported")]
        public ActionResult ReportedRecipe(int succError)
        {
            if(succError == 1)
            {
                ViewBag.MyString = 1;
                return View();
            }
            else
            {
                ViewBag.MyString = 0;
                return View();
            }
        }

        public ActionResult ApproveRecipe(int? id)
        {
            var r = m.RecipeGetById(id.GetValueOrDefault());
            if(r == null)
            {
                return HttpNotFound();
            }
            if (r.verified)
            {
                //Cannot approve verified recipes
                return RedirectToAction("Index");
            }

            bool error = m.ApproveRecipe(r.recipe_Id);
            if (!error)
            {
                return RedirectToAction("Details", new { id = r.recipe_Id, success = "Recipe has been verified. An email is sent to the author of the recipe" });
            }
            else
            {
                return RedirectToAction("Details", new { id = r.recipe_Id, bookMarkError = "An error while approving this recipe" });
            }
        }
    }
}