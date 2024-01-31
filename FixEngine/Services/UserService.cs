using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Resources;
using QuickFix.FIX44;

namespace FixEngine.Services
{
    public class UserService
    {
        private ILogger<UserService> _logger;    
        private DatabaseContext _context;
        private readonly string _pepper;
        private readonly int _iteration = 3;
        private readonly IConfiguration _configuration;
        public UserService(ILogger<UserService> logger, DatabaseContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _pepper = configuration.GetValue<string>("PasswordPepper");
        }
        public Models.User FetchUserByEmail(string email)
        {
            var user = _context.users.FirstOrDefault(x => x.Email == email) ;
            if(user == null)
            {
                return null;
            }
            else
            {
                return new Models.User()
                {
                    Id = user.Id,
                    Email = email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                };
            }
        } 
        public List<Models.User> FetchUsers() {
            return new List<Models.User>();
        }
        public void UpdateUser(Models.User user) { }
        public void DeleteUser(Models.User user) { }
        public bool AddUser(CreateUserRequestModel user) {

            bool isExist = _context.users.Any(x => x.Email == user.Email);
            if(isExist)
            {
                string message = $"User with email {user.Email} already exists";
                _logger.LogError(message);
                return false;
            }
            var newUser = new Entity.User()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PasswordSalt = PasswordHasher.GenerateSalt()
            };
            newUser.PasswordHash = PasswordHasher.ComputeHash(user.Password, newUser.PasswordSalt, _pepper, _iteration);
            _logger.LogInformation("Createing user: ", user.FirstName);
            _context.users.Add(newUser);
            _context.SaveChanges();
            return true;
        }
        public UserResource GetUser(string email, string password) {
            var user = _context.users.FirstOrDefault(x => x.Email == email);
            if(user == null)
            {
                return null;
            }
            var hash = PasswordHasher.ComputeHash(password, user.PasswordSalt, _pepper, _iteration);
            if (user.PasswordHash == hash) { }
            return new UserResource(user.Id, user.Email, user.FirstName, user.LastName);
        }
    }
}
