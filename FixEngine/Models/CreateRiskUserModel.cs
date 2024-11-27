using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class CreateRiskUserModel
    {
        [MaxLength(250)]
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public decimal Balance { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        public int Leverage { get; set; }
        public int GroupId { get; set; }
    }
}
