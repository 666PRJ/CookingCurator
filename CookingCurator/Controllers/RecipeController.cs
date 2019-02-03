using CookingCurator.EntityModels;
using System;
using System.Collections.Generic;
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
            var form = new RecipeAddViewForm();

            form.ingredList = new MultiSelectList(items: m.IngredGetAll(),
                dataValueField: "ingred_ID",
                dataTextField: "ingred_Name"
                );

            return View(form);
        }

        // GET: Recipe/Create
        public ActionResult CreateVerified()
        {
            var form = new RecipeVerifiedAddViewModel();

            form.ingredList = new MultiSelectList(items: m.IngredGetAll(),
                dataValueField: "ingred_ID",
                dataTextField: "ingred_Name"
                );

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
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Recipe/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
