using Microsoft.AspNetCore.Identity;
namespace utad.reFresh.core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        
        public ICollection<UserIngredient> UserIngredients { get; set; }
    }
}
