using FixEngine.Enums;
using FixEngine.Models;

namespace FixEngine.Services
{
    public interface IAuthService
    {
        Task<string> Login(string email, string password);
        Task<RegisterState> Register(CreateUserRequestModel userModel);
    }
}
