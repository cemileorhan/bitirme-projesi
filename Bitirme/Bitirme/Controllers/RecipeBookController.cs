using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bitirme.Models;
using Bitirme.Areas.Identity.Data;

namespace Bitirme.Controllers
{
    [Authorize]
    public class RecipeBookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipeBookController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var favoriteRecipes = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Recipe)
                .ToListAsync();

            var favoriteCategories = favoriteRecipes
                .Select(f => f.Recipe.CategoryId)
                .Distinct()
                .ToList();

            var recommendedRecipes = await _context.Recipes
                .Where(r => favoriteCategories.Contains(r.CategoryId) &&
                            !_context.Favorites.Any(f => f.UserId == userId && f.RecipeId == r.Id))
                .OrderBy(r => Guid.NewGuid()) // Rastgele sıralama
                .Take(30) // İlk 30 öneri
                .ToListAsync();

            ViewBag.RecommendedRecipes = recommendedRecipes;
            return View(favoriteRecipes);
        }




    }
}
