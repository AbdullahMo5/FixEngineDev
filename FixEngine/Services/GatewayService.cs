using FixEngine.Data;
using FixEngine.Entity;
using Microsoft.EntityFrameworkCore;

namespace FixEngine.Services
{
    public class GatewayService : GenericService<Gateway>, IGatewayService
    {
        private readonly DatabaseContext _context;

        public GatewayService(DatabaseContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsGatewayExistAsync(int gatewayId)
            => await _context.Gateways.AnyAsync(g => g.Id == gatewayId);
    }
}
