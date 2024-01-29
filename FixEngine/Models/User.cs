using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
