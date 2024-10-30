using FixEngine.Entity;
using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class CreateGateWayModel
    {
        [MaxLength(250)]
        public string Name { get; set; }
        public GatewayType Type { get; set; }
    }
}
