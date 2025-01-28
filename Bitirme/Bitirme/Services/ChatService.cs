using Bitirme.Areas.Identity.Data;
using Bitirme.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bitirme.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpClient _httpClient;

        public ChatService(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GetIntentFromOpenAI(string prompt)
        {
            var apiKey = ""; 
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an assistant that helps analyze user questions for recipe search or cooking advice. Keep all responses concise and precise." },
                    new { role = "user", content = $"""
                        Analyze the question below and extract:
                        1. The intent (e.g., "search by category", "search by ingredient", "recipe steps", "advice").
                        2. The main entity (e.g., a category like "Soup", an ingredient like "Chocolate", or a topic like "dry bread").
                        Provide the result as a concise JSON object with "intent" and "entity" fields.

                        Question: "{prompt}"
                        """ }
                },
                temperature = 0.2
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }

            Console.WriteLine($"OpenAI API Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return string.Empty;
        }

        public async Task<string> GetAdviceResponse(string topic)
        {
            var apiKey = ""; 
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = "You are a helpful assistant providing cooking and baking advice. Keep your answers short and precise, no longer than 1-2 sentences." },
            new { role = "user", content = $"Provide a clear, concise answer for the following question: '{topic}'. Focus on practical advice." }
        },
                temperature = 0.7
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                return parsedResponse?.choices?[0]?.message?.content?.ToString()?.Trim() ?? "Sorry, I couldn't generate advice.";
            }

            Console.WriteLine($"OpenAI API Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return "Sorry, I couldn't generate advice.";
        }


        public async Task<List<Recipe>> GetRecipesByCategory(string categoryName)
        {
            return await _dbContext.Recipes
                .Include(r => r.Category)
                .Where(r => r.Category.Name.ToLower().Contains(categoryName.ToLower()))
                .ToListAsync();
        }

        public async Task<List<Recipe>> GetRecipesByIngredient(string ingredientName)
        {
            return await _dbContext.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.RecipeIngredients.Any(ri => ri.Ingredient.Name.ToLower().Contains(ingredientName.ToLower())))
                .ToListAsync();
        }

        public async Task<List<RecipeStep>> GetRecipeSteps(string recipeName)
        {
            return await _dbContext.RecipeSteps
                .Include(rs => rs.Recipe)
                .Where(rs => rs.Recipe.Title.ToLower().Contains(recipeName.ToLower()))
                .OrderBy(rs => rs.StepNumber)
                .ToListAsync();
        }
    }
}
