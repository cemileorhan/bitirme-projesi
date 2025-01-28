using Bitirme.Areas.Identity.Data;

namespace Bitirme.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PreparationTime { get; set; } 
        public int CookingTime { get; set; } 
        public int Servings { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public string? QrCodeUrl { get; set; } 
        public byte[]? QrCodeImage { get; set; } 
        public string ImageUrl { get; set; } 
        public string DifficultyLevel { get; set; } 
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string? CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }
        public ICollection<RecipeStep> Steps { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Rating> Ratings { get; set; }

    }


}
