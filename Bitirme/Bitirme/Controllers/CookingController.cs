using Microsoft.AspNetCore.Mvc;
using Bitirme.Models;
using Microsoft.EntityFrameworkCore;
using Bitirme.Areas.Identity.Data;

namespace Bitirme.Controllers
{
    public class CookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Start(int recipeId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Steps.OrderBy(s => s.StepNumber))
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                return NotFound();
            }

            return RedirectToAction("Step", new { recipeId, stepNumber = 1 });
        }

        public async Task<IActionResult> Step(int recipeId, int stepNumber)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                return NotFound();
            }

            var step = recipe.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);

            if (step == null)
            {
                return RedirectToAction("Completed", new { recipeId });
            }

            // Tarifin QR kodu ile ilişkili video linki
            var videoUrl = recipe.QrCodeUrl.Replace("watch?v=", "embed/");

            if (step.VideoStart.HasValue && step.VideoEnd.HasValue)
            {
                videoUrl = $"{videoUrl}?start={step.VideoStart.Value}&end={step.VideoEnd.Value}";
            }

            ViewBag.VideoUrl = videoUrl;
            ViewBag.TotalSteps = recipe.Steps.Count; 

            return View(step);
        }


        public async Task<IActionResult> Completed(int recipeId)
        {
            ViewBag.RecipeId = recipeId;

            var randomRecipes = await _context.Recipes
                .OrderBy(r => Guid.NewGuid())  
                .Take(8)                      
                .ToListAsync();

            ViewBag.RandomRecipes = randomRecipes;  

            return View();
        }

    }
}
