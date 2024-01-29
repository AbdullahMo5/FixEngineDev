using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Models;

namespace FixEngine.Services
{
    public class UserService
    {
        private ILogger<UserService> _logger;    
        private DatabaseContext _context;

        public UserService(ILogger<UserService> logger, DatabaseContext context)
        {
            _logger = logger;
            _context = context;
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
        public void AddUser(CreateUserRequestModel user) {
            bool isExist = _context.users.Any(x => x.Email == user.Email);
            if(isExist)
            {
                string message = $"User with email {user.Email} already exists";
                _logger.LogError(message);
                return;
            }
            var newUser = new Entity.User()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Password = user.Password
            };
            _context.users.Add(newUser);
            _context.SaveChanges();
        }
    }
}
