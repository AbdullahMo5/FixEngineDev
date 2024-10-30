using System.ComponentModel.DataAnnotations;

namespace FixEngine.Entity
{
    public class Group
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string Name { get; set; }
        public int GatewayId { get; set; }
        public string CreatedBy { get; set; }
        public Gateway Gateway { get; set; }
    }
}
