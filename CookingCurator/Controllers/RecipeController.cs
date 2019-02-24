﻿using CookingCurator.EntityModels;
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
        public ActionResult Index(string countryName, string mealType, string verified, int? page)
        {
            var recipes = m.RecipeGetAll();
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
                     recipes = m.FilterRecipesByCountry(countryName);
                }
            }
            if (!string.IsNullOrEmpty(verified))
            {
                if (!verified.Equals("0"))
                {
                    recipes = m.FilterVerifiedRecipes(verified, recipes);
                }
            }

            return View(recipes);
        }
        // GET: Recipe/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            var recipe = m.RecipeWithIngredGetById(id.GetValueOrDefault());
            recipe.ingreds = m.ingredsForRecipeViewModel(id.GetValueOrDefault());
            if (recipe == null)
                return HttpNotFound();
            else
                return View(recipe);
        }

        // GET: Recipe/Create
        [Authorize]
        public ActionResult Create()
        {
            var form = new RecipeAddViewForm();

            form.ingredients = m.IngredientGetAll();
            form.selectedIngredsId = new string[0];
            return View(form);
        }

        // GET: Recipe/Create
        [Authorize(Roles = "Admin")]
        public ActionResult CreateVerified()
        {
            var form = new RecipeVerifiedAddViewModel();

            form.ingredients = m.IngredientGetAll();
            form.selectedIngredsId = new string[0];

            return View(form);
        }

        //CreateVerified will be only avilable to admin
        // POST: Recipe/Create
        [HttpPost]
        public ActionResult CreateVerified(RecipeVerifiedAddViewModel newItem)
        {
            // Validate the input
            if (!ModelState.IsValid)
                return View(newItem);

            try
            {
                // Process the input
                newItem.verified = true;
                newItem.rating = 0;
                newItem.lastUpdated = DateTime.Now;
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
                return View(newItem);
            }
        }

        // POST: Recipe/Create
        [HttpPost]
        public ActionResult Create(RecipeAddViewForm newItem)
        {
            // Validate the input
            if (!ModelState.IsValid)
                return View(newItem);

            try
            {
                // Process the input
                newItem.verified = false;
                newItem.rating = 0;
                newItem.lastUpdated = DateTime.Now;
                var addedItem = m.RecipeAdd(newItem);

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
                return View(newItem);
            }
        }

        // GET: Recipe/Edit/5
        public ActionResult Edit(int? id)
        {
            Recipe_IngredViewModel recipes = new Recipe_IngredViewModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeBaseViewModel recipe = m.RecipeGetById(id);
            IEnumerable<IngredientBaseViewModel> ingredients = m.IngredientGetAll();
            String[] selectedIngreds = m.ingredsForRecipe(id).ToArray();
            if (recipe == null)
            {
                return HttpNotFound();
            }
            recipes.recipe = recipe;
            recipes.ingredients = ingredients;
            recipes.selectedIngredsId = selectedIngreds;
            return View(recipes);
        }

        // POST: Recipe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Recipe_IngredViewModel recipes)
        {
            if (!ModelState.IsValid)
            {
                return View(recipes);
            }

            try
            {

                var editedrecipe = m.RecipeEdit(recipes);

                if (editedrecipe == null)
                {
                    return View(recipes);
                }
                else
                {
                    return RedirectToAction("Details", new { id = editedrecipe.recipe_Id });
                }
            }
            catch
            {
                return View(recipes);
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
            catch (DbEntityValidationException vex)
            {
                foreach (var error in vex.EntityValidationErrors)
                {
                    foreach (var errorMsg in error.ValidationErrors)
                    {
                        // logging service based on NLog
                        Console.WriteLine($"Error trying to save EF changes - {errorMsg.ErrorMessage}");
                    }
                }

                throw;
            }
            catch (DbUpdateException dbu)
            {
                var exception = HandleDbUpdateException(dbu);
                throw exception;
            }
            return RedirectToAction("Index");
        }
        private Exception HandleDbUpdateException(DbUpdateException dbu)
        {
            var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");

            try
            {
                foreach (var result in dbu.Entries)
                {
                    builder.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
                }
            }
            catch (Exception e)
            {
                builder.Append("Error parsing DbUpdateException: " + e.ToString());
            }

            string message = builder.ToString();
            return new Exception(message, dbu);
        }

        [Route("User/AuthorProfile")]
        public ActionResult Authors(string authorName)
        {
            var r = m.RecipesByAuthor(authorName);
            return View(r);
        }

        public ActionResult BookmarkRecipe(int? ID)
        {
            if(ID == null)
            {
                return View("Details", new { id = ID});
            }
            var error = m.BookMarkRecipe(ID.GetValueOrDefault());
            if(error == 0)
            {
                ViewBag.MyString = 0;
                return View();
            }
            else
            {
                ViewBag.MyString = 1;
                return View();
            }
        }
    }
}