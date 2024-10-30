using FixEngine.Data;
using FixEngine.Entity;

namespace FixEngine.Services
{
    public class GatewayService : GenericService<Gateway>, IGatewayService
    {
        public GatewayService(DatabaseContext context) : base(context) { }
    }
}
