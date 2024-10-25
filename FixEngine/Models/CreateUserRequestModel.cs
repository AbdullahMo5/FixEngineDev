using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class CreateUserRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }
        [MaxLength(250)]
        public string FirstName { get; set; }
        [MaxLength(250)]
        public string LastName { get; set; }
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
