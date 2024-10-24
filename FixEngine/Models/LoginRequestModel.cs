using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class LoginRequestModel
    {
        [EmailAddress(ErrorMessage = "Ivalid Email")]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
