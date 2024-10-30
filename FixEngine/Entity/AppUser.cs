using Microsoft.AspNetCore.Identity;

namespace FixEngine.Entity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<RiskUser> RiskUsers { get; set; } = new List<RiskUser>();
    }
}
