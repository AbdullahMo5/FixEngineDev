using FixEngine.Data;
using FixEngine.Entity;
using Microsoft.EntityFrameworkCore;

namespace FixEngine.Services
{
    public class RiskUserService : GenericService<RiskUser>, IRiskUserService
    {
        private readonly DatabaseContext _context;

        public RiskUserService(DatabaseContext context) : base(context)
        {
            _context = context;
        }

        public GatewayType GetGatewayType(int id)
            => (from q in _context.RiskUsers.AsNoTracking()
                where q.Id == id
                select q.Group.Gateway.Type).FirstOrDefault();
    }
}
