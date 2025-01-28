using Bitirme.Areas.Identity.Data;
using Bitirme.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Bitirme.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeStep> RecipeSteps { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<ChatBotLog> ChatBotLogs { get; set; }
    public DbSet<Rating> Ratings { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatBotLog>().HasNoKey();

        builder.Entity<Comment>()
            .HasOne(c => c.Recipe)
            .WithMany(r => r.Comments)
            .HasForeignKey(c => c.RecipeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorite>()
            .HasOne(f => f.Recipe)
            .WithMany(r => r.Favorites)
            .HasForeignKey(f => f.RecipeId)
            .OnDelete(DeleteBehavior.NoAction); 

        builder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.Entity<Rating>()
            .HasOne(r => r.Recipe)
            .WithMany(re => re.Ratings)
            .HasForeignKey(r => r.RecipeId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }


}
public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(255);
        builder.Property(u => u.LastName).HasMaxLength(255);

    }
}
