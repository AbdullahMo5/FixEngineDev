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
        private DatabaseContext _context;//to-do: remove
        private UserService _userService;

        public LoginService(ILogger<LoginService> logger, DatabaseContext context, SessionManager sessionManager, UserService userService)
        {
            _logger = logger;
            _sessionManager = sessionManager;
            _context = context;
            _userService = userService;
        }
        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
        public bool Authenticate(string email, string password) {
            var user = _userService.GetUser(email, password);//_context.users.FirstOrDefault(x => x.Email == email);
            if (user == null) {
                return false;
            }
            else
            {
                return true;// user.Password == password;
            }
        }
        public string Login(string email, string password)
        {
            var user = _userService.GetUser(email, password);

            bool isExist = (user != null /*&& user.Password == password*/);// Authenticate(email, password);
            if (isExist)
            {
                //var hash = PasswordHasher.ComputeHash(password, user.PasswordSalt,);
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
