using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace FixEngine.Entity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonIgnore]
        public List<RiskUser> RiskUsers { get; set; } = new List<RiskUser>();
    }
}
