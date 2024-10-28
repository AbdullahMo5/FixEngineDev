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
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[\\W_]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and includes at least one uppercase letter, one lowercase letter, one digit, and one special character (e.g., !, @, #, $).")]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
