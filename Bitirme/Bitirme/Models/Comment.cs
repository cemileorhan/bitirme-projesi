using Bitirme.Areas.Identity.Data;

namespace Bitirme.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; } 
        public DateTime? DateCreated { get; set; } = DateTime.Now;
    }

}
