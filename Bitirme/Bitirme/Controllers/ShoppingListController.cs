using Bitirme.Areas.Identity.Data;
using Bitirme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bitirme.Controllers
{
    public class ShoppingListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShoppingListController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var shoppingList = _context.ShoppingLists
                .Include(s => s.Ingredient)
                .Where(s => s.UserId == userId)
                .ToList();

            var ingredientIds = shoppingList
                .Select(s => s.IngredientId)
                .Distinct()
                .ToList();

            var recommendedRecipes = _context.Recipes
                .Include(r => r.RecipeIngredients) 
                .Where(r => r.RecipeIngredients.Any(ri => ingredientIds.Contains(ri.IngredientId)))
                .OrderBy(r => Guid.NewGuid()) 
                .Take(20) 
                .ToList();

            ViewBag.RecommendedRecipes = recommendedRecipes;

            return View(shoppingList);
        }


        [HttpPost]
        public IActionResult AddToShoppingList(int ingredientId, string quantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var shoppingListItem = new ShoppingList
                {
                    UserId = userId,
                    IngredientId = ingredientId,
                    Quantity = quantity,
                    DateAdded = DateTime.Now
                };

                _context.ShoppingLists.Add(shoppingListItem);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Ingredient added to the list successfully!";
                return RedirectToAction("Details", new { id = Request.Form["recipeId"] }); 
            }

            TempData["ErrorMessage"] = "Failed to add ingredient.";
            return RedirectToAction("Details", new { id = Request.Form["recipeId"] });
        }

        [HttpPost]
        public IActionResult RemoveFromShoppingList(int shoppingListItemId)
        {
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var item = _context.ShoppingLists
                .FirstOrDefault(s => s.Id == shoppingListItemId && s.UserId == userId);

            if (item != null)
            {
                _context.ShoppingLists.Remove(item);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Ingredient successfully removed from the list.";
            }
            else
            {
                TempData["ErrorMessage"] = "Ingredient not found in your list.";
            }

            return RedirectToAction("Index");
        }




    }
}
