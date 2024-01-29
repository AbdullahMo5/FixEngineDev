using FixEngine.Controllers;
using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Shared;
using Services;

namespace FixEngine.Services
{
    public class LoginService
    {
        private readonly ILogger<LoginService> _logger;
        private SessionManager _sessionManager;
        private DatabaseContext _context;

        public LoginService(ILogger<LoginService> logger, DatabaseContext context, SessionManager sessionManager)
        {
            _logger = logger;
            _sessionManager = sessionManager;
            _context = context;
        }
        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
        public bool Authenticate(string email, string password) { 
            var user = _context.users.FirstOrDefault(x => x.Email == email);
            if (user == null) {
                return false;
            }
            else
            {
                return user.Password == password;
            }
        }
        public string Login(string email, string password)
        {
            var user = _context.users.FirstOrDefault(x => x.Email == email);
            bool isExist = (user != null && user.Password == password);// Authenticate(email, password);
            if (isExist)
            {
                string token = GenerateToken();
                _sessionManager.AddSession(token, user);
                return token;
                
            }
            return "";
        }
        public void Logout(string token) {
            _sessionManager.RemoveSession(token);   
            //var user = _context.users.FirstOrDefault();
        }
    }
}
