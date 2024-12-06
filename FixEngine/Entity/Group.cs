using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FixEngine.Entity
{
    public class Group
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string Name { get; set; }
        public int GatewayId { get; set; }
        public int MarginCall { get; set; }
        public int StopOut { get; set; }
        public string CreatedBy { get; set; }
        public Gateway Gateway { get; set; }
        [JsonIgnore]
        public List<RiskUser> RiskUsers { get; set; } = new List<RiskUser>();
    }
}
