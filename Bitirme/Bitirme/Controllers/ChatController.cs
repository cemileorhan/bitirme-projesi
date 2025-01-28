using Microsoft.AspNetCore.Mvc;
using Bitirme.Services;
using Newtonsoft.Json;

namespace Bitirme.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message cannot be empty.");

            var aiResponse = await _chatService.GetIntentFromOpenAI(request.Message);

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                return Ok(new { response = "Sorry, I couldn't understand your question." });
            }

            try
            {
                var parsedResponse = JsonConvert.DeserializeObject<dynamic>(aiResponse);
                var choice = parsedResponse?.choices?[0]?.message?.content?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(choice))
                {
                    return Ok(new { response = "Sorry, I couldn't understand your question." });
                }

                var intentData = JsonConvert.DeserializeObject<dynamic>(choice);
                string intent = intentData?.intent ?? "advice";
                string entity = intentData?.entity ?? "ingredient substitution";

                if (string.IsNullOrWhiteSpace(intent) || string.IsNullOrWhiteSpace(entity))
                {
                    Console.WriteLine("Intent veya entity bulunamadı. Varsayılan değerlere geçildi.");
                    intent = "advice";
                    entity = request.Message; 
                }

                if (intent == "advice")
                {
                    if (string.IsNullOrWhiteSpace(entity))
                    {
                        Console.WriteLine("Entity is empty, defaulting to full prompt as topic.");
                        entity = request.Message; 
                    }

                    var adviceResponse = await _chatService.GetAdviceResponse(entity);
                    return Ok(new { response = adviceResponse });
                }

                else if (intent == "recipe steps")
                {
                    var steps = await _chatService.GetRecipeSteps(entity);
                    if (steps.Any())
                    {
                        return Ok(new
                        {
                            response = $"Steps for {entity}: {string.Join(", ", steps.Select(s => $"{s.StepNumber}. {s.Description}"))}"
                        });
                    }

                    return Ok(new { response = $"I couldn't find any steps for {entity}." });
                }
                else if (intent == "search by category")
                {
                    var recipes = await _chatService.GetRecipesByCategory(entity);
                    if (recipes.Any())
                    {
                        return Ok(new
                        {
                            response = $"Recipes for {entity}: {string.Join(", ", recipes.Take(3).Select(r => r.Title))}" 
                        });
                    }

                    return Ok(new { response = $"No recipes found for {entity}." });
                }
                else if (intent == "search by ingredient")
                {
                    var recipes = await _chatService.GetRecipesByIngredient(entity);
                    if (recipes.Any())
                    {
                        return Ok(new
                        {
                            response = $"Recipes with {entity}: {string.Join(", ", recipes.Take(3).Select(r => r.Title))}" 
                        });
                    }

                    return Ok(new { response = $"No recipes found with {entity}." });
                }

                return Ok(new { response = "Sorry, I couldn't understand your request." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing AI response: {ex.Message}");
                return Ok(new { response = "Sorry, something went wrong while processing your request." });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
