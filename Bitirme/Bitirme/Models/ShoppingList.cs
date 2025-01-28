using Bitirme.Areas.Identity.Data;

namespace Bitirme.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public string Quantity { get; set; } 
        public DateTime? DateAdded { get; set; } = DateTime.Now;
    }

}
