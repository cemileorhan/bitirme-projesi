﻿using Bitirme.Areas.Identity.Data;

namespace Bitirme.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int Value { get; set; } 
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
