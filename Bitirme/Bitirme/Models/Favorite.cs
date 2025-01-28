using Bitirme.Areas.Identity.Data;

namespace Bitirme.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public DateTime? DateAdded { get; set; } = DateTime.Now;
    }

}
