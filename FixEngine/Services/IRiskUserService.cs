using FixEngine.Entity;

namespace FixEngine.Services
{
    public interface IRiskUserService : IGenericService<RiskUser>
    {
        GatewayType GetGatewayType(int id);
    }
}
