using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitirme.Models;
using Microsoft.AspNetCore.Identity;

namespace Bitirme.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Favorite> Favorites { get; set; }
    public ICollection<Rating> Ratings { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
}

