using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class CreateUserRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get;set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
