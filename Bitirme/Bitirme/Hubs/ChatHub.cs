using Bitirme.Areas.Identity.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Bitirme.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string userMessage)
        {
            string reply = await GenerateResponse(userMessage);
            await Clients.Caller.SendAsync("ReceiveMessage", reply);
        }

        private async Task<string> GenerateResponse(string userMessage)
        {
            try
            {
                userMessage = userMessage.ToLower(); // Küçük harfe çevir

                // Malzeme bazlı tarifler
                if (userMessage.Contains("with") || userMessage.Contains("includes"))
                {
                    string ingredient = ExtractIngredient(userMessage);
                    return await GetRecipesByIngredient(ingredient);
                }

                // Eksik malzeme
                else if (userMessage.Contains("don't have") || userMessage.Contains("missing"))
                {
                    string missingIngredient = ExtractIngredient(userMessage);
                    return await GetSuggestionsForMissingIngredient(missingIngredient);
                }

                // Zorluk seviyesi
                else if (userMessage.Contains("easy") || userMessage.Contains("hard") || userMessage.Contains("advanced"))
                {
                    string difficulty = ExtractDifficulty(userMessage);
                    return await GetRecipesByDifficulty(difficulty);
                }

                // Kategoriye göre tarifler
                else if (userMessage.Contains("dessert") || userMessage.Contains("breakfast") || userMessage.Contains("dinner"))
                {
                    string category = ExtractCategory(userMessage);
                    return await GetRecipesByCategory(category);
                }

                // Tarif süresi
                else if (userMessage.Contains("minutes") || userMessage.Contains("quick") || userMessage.Contains("fast"))
                {
                    int? maxTime = ExtractCookingTime(userMessage);
                    return await GetRecipesByTime(maxTime);
                }

                // Anlaşılmayan sorular
                else
                {
                    return "I'm sorry, I didn't understand your question. Could you ask something like 'Can you recommend a recipe with chicken?' or 'Show me some easy recipes.'?";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return $"An error occurred: {ex.Message}. Please try again later.";
            }
        }

        // Malzeme bazlı tarifler
        private async Task<string> GetRecipesByIngredient(string ingredient)
        {
            if (string.IsNullOrEmpty(ingredient))
            {
                return "Could you please specify an ingredient?";
            }

            var recipes = await _context.RecipeIngredients
                .Include(ri => ri.Recipe)
                .Where(ri => EF.Functions.Like(ri.Ingredient.Name, $"%{ingredient}%"))
                .Select(ri => ri.Recipe.Title)
                .Distinct()
                .Take(5)
                .ToListAsync();

            return recipes.Any()
                ? $"Here are some recipes with {ingredient}: {string.Join(", ", recipes)}"
                : $"I'm sorry, I couldn't find any recipes that include {ingredient}.";
        }

        // Eksik malzeme önerileri
        private async Task<string> GetSuggestionsForMissingIngredient(string missingIngredient)
        {
            if (string.IsNullOrEmpty(missingIngredient))
            {
                return "Could you please specify the ingredient you are missing?";
            }

            return $"If you're missing {missingIngredient}, try searching for recipes like 'no-{missingIngredient}' or using common substitutes.";
        }

        // Zorluk seviyesine göre tarifler
        private async Task<string> GetRecipesByDifficulty(string difficulty)
        {
            var recipes = await _context.Recipes
                .Where(r => r.DifficultyLevel.ToLower() == difficulty.ToLower())
                .Select(r => r.Title)
                .Take(5)
                .ToListAsync();

            return recipes.Any()
                ? $"Here are some {difficulty} recipes: {string.Join(", ", recipes)}"
                : $"I'm sorry, I couldn't find any {difficulty} recipes.";
        }

        // Kategoriye göre tarifler
        private async Task<string> GetRecipesByCategory(string category)
        {
            // Kategoriyi ve tarifleri çekiyoruz
            var recipes = await _context.Recipes
                .Include(r => r.Category) // Kategori ilişkisini dahil ediyoruz
                .Where(r => r.Category.Name.ToLower().Trim() == category.ToLower().Trim()) // Harf duyarsız kontrol
                .Select(r => r.Title) // Tarif başlıklarını alıyoruz
                .Take(5) // Maksimum 5 tarif getiriyoruz
                .ToListAsync();

            return recipes.Any()
                ? $"Here are some recipes in the {category} category: {string.Join(", ", recipes)}"
                : $"I'm sorry, I couldn't find any recipes in the {category} category. Please make sure the category exists or try another one.";
        }



        // Tarif süresine göre tarifler
        private async Task<string> GetRecipesByTime(int? maxTime)
        {
            if (maxTime == null)
            {
                return "Could you please specify the maximum cooking time in minutes?";
            }

            var recipes = await _context.Recipes
                .Where(r => (r.PreparationTime + r.CookingTime) <= maxTime)
                .Select(r => r.Title)
                .Take(5)
                .ToListAsync();

            return recipes.Any()
                ? $"Here are some recipes you can cook in {maxTime} minutes or less: {string.Join(", ", recipes)}"
                : $"I'm sorry, I couldn't find any recipes that can be cooked in {maxTime} minutes or less.";
        }

        // Yardımcı metotlar

        // Malzeme çıkarımı
        private string ExtractIngredient(string message)
        {
            string[] words = message.Split(' ');
            int index = Array.IndexOf(words, "with");

            if (index == -1)
            {
                index = Array.IndexOf(words, "includes");
            }

            if (index >= 0 && index + 1 < words.Length)
            {
                return words[index + 1].TrimEnd(new char[] { '.', '?' });
            }

            return string.Empty;
        }

        // Zorluk seviyesi çıkarımı
        private string ExtractDifficulty(string message)
        {
            if (message.Contains("easy")) return "easy";
            if (message.Contains("hard")) return "hard";
            if (message.Contains("advanced")) return "advanced";
            return string.Empty;
        }

        // Kategori çıkarımı
        private string ExtractCategory(string message)
        {
            if (message.Contains("dessert")) return "dessert";
            if (message.Contains("breakfast")) return "breakfast";
            if (message.Contains("dinner")) return "dinner";
            return string.Empty;
        }

        // Süre çıkarımı
        private int? ExtractCookingTime(string message)
        {
            var words = message.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int time))
                {
                    return time;
                }
            }

            return null;
        }
    }
}
