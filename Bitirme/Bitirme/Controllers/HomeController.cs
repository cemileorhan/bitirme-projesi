using Bitirme.Areas.Identity.Data;
using Bitirme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using QRCoder;
using SkiaSharp;

namespace Bitirme.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            var recipes = await _context.Recipes.ToListAsync();
            
            var favoriteRecipeIds = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.RecipeId)
                .ToListAsync();

            ViewBag.FavoriteRecipeIds = favoriteRecipeIds;
            return View(recipes);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Steps)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.User)
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Average Rating
            double averageRating = recipe.Ratings.Any()
                ? recipe.Ratings.Average(r => r.Value)
                : 0;

            ViewData["AverageRating"] = averageRating.ToString("0.0");
            ViewData["AverageRatingValue"] = averageRating;

            // Yorumlarý tarihe göre sýralams
            var orderedComments = recipe.Comments
                .OrderByDescending(c => c.DateCreated)
                .ToList();
            ViewData["OrderedComments"] = orderedComments;

            return View(recipe);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCommentAndRating(int recipeId, string commentText, int ratingValue)
        {
            if (ratingValue < 1 || ratingValue > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Details", new { id = recipeId });
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["ErrorMessage"] = "Comment text cannot be empty.";
                return RedirectToAction("Details", new { id = recipeId });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = new Comment
            {
                RecipeId = recipeId,
                UserId = user.Id,
                Text = commentText,
                DateCreated = DateTime.Now
            };
            _context.Comments.Add(comment);

            // Yeni veya güncellenmiþ rating ekle
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == user.Id);

            if (existingRating != null)
            {
                existingRating.Value = ratingValue;
                existingRating.DateCreated = DateTime.Now;
            }
            else
            {
                var rating = new Rating
                {
                    RecipeId = recipeId,
                    UserId = user.Id,
                    Value = ratingValue,
                    DateCreated = DateTime.Now
                };
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your comment and rating have been successfully added!";
            return RedirectToAction("Details", new { id = recipeId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                _logger.LogWarning("Comment not found for ID: {CommentId}", commentId);
                return NotFound();
            }

            // Kullanýcý sadece kendi yorumunu silebilir
            if (comment.UserId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                _logger.LogWarning("Unauthorized delete attempt by user ID: {UserId} for comment ID: {CommentId}",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, commentId);
                return Unauthorized();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment ID: {CommentId} deleted successfully", commentId);
            return RedirectToAction("Details", new { id = comment.RecipeId });
        }


        public async Task<IActionResult> CategoryRecipes(int categoryId)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            var recipes = await _context.Recipes
                .Include(r => r.Category)
                .Where(r => r.CategoryId == categoryId)
                .ToListAsync();

            ViewData["CategoryName"] = _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => c.Name)
                .FirstOrDefault();

            return View("CategoryRecipes", recipes);
        }

        [HttpPost]
        public IActionResult AddToShoppingList(int recipeId, int ingredientId, string quantity)
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
                return RedirectToAction("Details", new { id = recipeId });
            }

            TempData["ErrorMessage"] = "Failed to add ingredient.";
            return RedirectToAction("Details", new { id = recipeId });
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites(int recipeId)
        {
            _logger.LogInformation("AddToFavorites API tetiklendi. RecipeId: {RecipeId}", recipeId);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (favorite == null)
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    DateAdded = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }

            return Ok(new { success = false });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddOrRemoveFavorite(int recipeId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Unauthorized" });
            }

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (existingFavorite == null)
            {
                // Favorilere ekle
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    DateAdded = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Ok(new { success = true, action = "added" });
            }
            else
            {
                // Favorilerden çýkar
                _context.Favorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, action = "removed" });
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GenerateRecipeQrCode(int recipeId)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                return NotFound(); 
            }

            if (string.IsNullOrEmpty(recipe.QrCodeUrl)) 
            {
                TempData["ErrorMessage"] = "No URL found in QrCodeUrl for this recipe.";
                return RedirectToAction("Details", new { id = recipeId });
            }

            var qrCodeImage = SkiaSharpQRCodeHelper.GenerateQrCode(recipe.QrCodeUrl);

            recipe.QrCodeImage = qrCodeImage;

            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "QR code generated successfully!";
            return RedirectToAction("Details", new { id = recipeId });
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
