using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FixEngine.Controllers
{
    [Route("api/public/[action]")]
    [ApiController]
    public class LoginController:ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private LoginService _service;

        public LoginController(ILogger<LoginController> logger, LoginService service)
        {
            _logger = logger;
            _service = service;
        }
        [HttpPost]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequestModel credentials)
        {
            string token = _service.Login(credentials.Email, credentials.Password);
            if(String.IsNullOrWhiteSpace(token))
                return NotFound("User not found");
            return Ok(token);
        }
        [HttpPost]
        public async Task<ActionResult> Logout() {
            return Ok();
        }
    }
}
