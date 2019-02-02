using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CookingCurator.Controllers
{
    public class RecipeController : Controller
    {
        private Manager m = new Manager();

        [Authorize]
        public ActionResult Index()
        {
            var recipes = m.RecipeGetAll();
            return View(recipes);
        }
        // GET: Recipe/Details/5
        public ActionResult Details(int? id)
        {
            var recipe = m.RecipeGetById(id.GetValueOrDefault());

            if (recipe == null)
                return HttpNotFound();
            else
                return View(recipe);
        }

        // GET: Recipe/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Recipe/Create
        [HttpPost]
        public ActionResult Create(RecipeAddViewModel newItem)
        {
            // Validate the input
            if (!ModelState.IsValid)
                return View(newItem);

            try
            {
                // Process the input

                var addedItem = m.RecipeAdd(newItem);

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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeBaseViewModel recipe = m.RecipeGetById(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = recipe.title;
            return View(recipe);
        }

        // POST: Recipe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, RecipeAddViewModel recipe)
        {
            if (!ModelState.IsValid)
            {
                return View(recipe);
            }

            try
            {

                var editedRecipe = m.RecipeEdit(id, recipe);

                if (editedRecipe == null)
                {
                    return View(recipe);
                }
                else
                {
                    return RedirectToAction("Details", new { id = editedRecipe.recipe_Id });
                }
            }
            catch
            {
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
            catch (DataException)
            {
                return RedirectToAction("Delete", ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.");
            }
            return RedirectToAction("Index");
        }
    }
}
