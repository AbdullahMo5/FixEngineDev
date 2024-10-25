using System.ComponentModel.DataAnnotations;

namespace FixEngine.Models
{
    public class CreateGroupModel
    {
        [MaxLength(250)]
        public string Name { get; set; }
        public int GatewayId { get; set; }
    }
}
