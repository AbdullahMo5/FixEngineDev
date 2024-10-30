
using FixEngine.Entity;
using FixEngine.Enums;
using FixEngine.Models;
using FixEngine.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FixEngine.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly SessionManager _sessionManager;
        private readonly SymmetricSecurityKey _key;

        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, SessionManager sessionManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _sessionManager = sessionManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
        }

        private string GenerateToken(AppUser user, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Role, role),
                 new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            //return Guid.NewGuid().ToString();
            return tokenHandler.WriteToken(token);
        }
        public async Task<string> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (!result.Succeeded)
                return null;
            var token = GenerateToken(user, "user");
            _sessionManager.AddSession(token, new Resources.UserResource(user.Id, email, user.FirstName, user.LastName));

            return token;
        }

        public async Task<RegisterState> Register(CreateUserRequestModel userModel)
        {
            var user = await _userManager.FindByEmailAsync(userModel.Email);

            if (user != null)
                return RegisterState.EmailAlreadyExist;

            var appUser = new AppUser
            {
                Email = userModel.Email,
                UserName = userModel.Email,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
            };

            var result = await _userManager.CreateAsync(appUser, userModel.Password);

            if (!result.Succeeded)
                return RegisterState.Failed;

            return RegisterState.Succeed;
        }
    }
}
