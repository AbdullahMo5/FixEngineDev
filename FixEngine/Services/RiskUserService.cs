using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FixEngine.Services
{
    public class RiskUserService : GenericService<RiskUser>, IRiskUserService
    {
        private readonly DatabaseContext _context;
        private readonly IPasswordHasher<RiskUser> _passwordHasher;
        private readonly SessionManager _sessionManager;
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;

        public RiskUserService(DatabaseContext context,
            IPasswordHasher<RiskUser> passwordHasher,
            SessionManager sessionManager,
            IConfiguration configuration) : base(context)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _sessionManager = sessionManager;
            _configuration = configuration;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
        }

        public GatewayType GetGatewayType(int id)
            => (from q in _context.RiskUsers.AsNoTracking()
                where q.Id == id
                select q.Group.Gateway.Type).FirstOrDefault();

        public async Task<RiskUser?> GetRiskUserByEmail(string email)
            => await _context.RiskUsers.AsNoTracking().FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());

        public async Task<string> Login(string email, string password)
        {
            var riskUser = await GetRiskUserByEmail(email);
            if (riskUser == null)
                return null;
            var res = _passwordHasher.VerifyHashedPassword(riskUser, riskUser.Password, password);
            if (res != PasswordVerificationResult.Success)
                return null;

            string token = GenerateToken(riskUser.Email, riskUser.Id.ToString(), "user");
            _sessionManager.AddSession(token, new Resources.UserResource(riskUser.Id.ToString(), email, "", "riskUser.LastName"));

            return token;
        }

        private string GenerateToken(string email, string id, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                 new Claim(ClaimTypes.Role, role),
                 new Claim(ClaimTypes.NameIdentifier, id)
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
            return tokenHandler.WriteToken(token);
        }
    }
}
