using System.Text.Json.Serialization;

namespace FixEngine.Entity
{
    public class RiskUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public decimal Balance { get; set; }
        public int Leverage { get; set; }
        public int GroupId { get; set; }
        public string AppUserId { get; set; }
        [JsonIgnore]
        public AppUser AppUser { get; set; }
        [JsonIgnore]
        public Group Group { get; set; }
    }
}
