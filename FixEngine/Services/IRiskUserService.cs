using FixEngine.Entity;

namespace FixEngine.Services
{
    public interface IRiskUserService : IGenericService<RiskUser>
    {
        GatewayType GetGatewayType(int id);
        Task<RiskUser?> GetRiskUserByEmail(string email);
        Task<string> Login(string email, string password);
    }
}
