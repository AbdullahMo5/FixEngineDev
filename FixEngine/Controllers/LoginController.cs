using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/public/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private LoginService _service;
        private readonly IAuthService _authService;

        public LoginController(ILogger<LoginController> logger, LoginService service, IAuthService authService)
        {
            _logger = logger;
            _service = service;
            _authService = authService;
        }
        [HttpPost]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequestModel credentials)
        {
            string token = await _authService.Login(credentials.Email, credentials.Password);
            if (String.IsNullOrWhiteSpace(token))
                return NotFound("User not found");
            return Ok(token);
        }
        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            return Ok();
        }
    }
}
