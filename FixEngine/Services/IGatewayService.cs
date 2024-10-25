using FixEngine.Entity;

namespace FixEngine.Services
{
    public interface IGatewayService : IGenericService<Gateway>
    {
        Task<bool> IsGatewayExistAsync(int gatewayId);
    }
}
