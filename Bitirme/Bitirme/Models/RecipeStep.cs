namespace Bitirme.Models
{
    public class RecipeStep
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int StepNumber { get; set; } 
        public string Description { get; set; }
        public string? MediaUrl { get; set; }
        public int? VideoStart { get; set; } 
        public int? VideoEnd { get; set; }   
    }

}
