using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using utad.reFresh.core.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public ApplicationDbContext()
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
            
        }
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    
    public DbSet<IngredientSearch> IngredientSearches { get; set; }
    
    public DbSet<UserIngredient> UserIngredients { get; set; }
    
    public DbSet<Equipment> Equipment { get; set; }
    
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<RecipeStep> RecipeSteps { get; set; }
    public DbSet<OpenFoodFactsProductSearch> OpenFoodFactsProductSearches { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RecipeIngredient: composite key
        builder.Entity<RecipeIngredient>()
            .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

        builder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(ri => ri.RecipeId);

        builder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany() // No navigation property on Ingredient
            .HasForeignKey(ri => ri.IngredientId);
        
        // RecipeStep: one-to-many
        builder.Entity<RecipeStep>()
            .HasOne(rs => rs.Recipe)
            .WithMany(r => r.Steps)
            .HasForeignKey(rs => rs.RecipeId);
        
        
        builder.Entity<IngredientSearch>()
            .HasMany(s => s.Ingredients)
            .WithMany(i => i.IngredientSearches);
        
        builder.Entity<UserIngredient>()
            .HasKey(ui => new { ui.UserId, ui.IngredientId });

        builder.Entity<UserIngredient>()
            .HasOne(ui => ui.User)
            .WithMany(u => u.UserIngredients)
            .HasForeignKey(ui => ui.UserId);

        builder.Entity<UserIngredient>()
            .HasOne(ui => ui.Ingredient)
            .WithMany(i => i.UserIngredients)
            .HasForeignKey(ui => ui.IngredientId);

    }

}