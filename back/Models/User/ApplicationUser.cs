using Microsoft.AspNetCore.Identity;

namespace Quizer.Models.User
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public bool IsTemporal { get; set; }
    }
}