using FixEngine.Data;
using FixEngine.Entity;

namespace FixEngine.Services
{
    public class RiskUserService : GenericService<RiskUser>, IRiskUserService
    {
        public RiskUserService(DatabaseContext context) : base(context) { }
    }
}
