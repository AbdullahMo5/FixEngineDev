using FixEngine.Enums;
using FixEngine.Models;
using FixEngine.Resources;
using FixEngine.Services;
using FixEngine.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SessionManager _sessionManager;

        public AuthController(IAuthService authService, SessionManager sessionManager)
        {
            _authService = authService;
            _sessionManager = sessionManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(CreateUserRequestModel createUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Register(createUser);

            if (result == RegisterState.EmailAlreadyExist)
                return BadRequest($"{createUser.Email} is already exist");

            if (result == RegisterState.Failed)
                return BadRequest("Something went wrong");

            return Ok("registration is successful");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestModel loginModel)
        {
            if (loginModel.Email.IsNullOrEmpty())
            {
                ModelState.AddModelError("message", "Email can not be null");
                return BadRequest(ModelState);
            }

            var token = await _authService.Login(loginModel.Email, loginModel.Password);

            if (token == null)
            {

                ModelState.AddModelError("message", "Email Or Password incorrect");
                return BadRequest(ModelState);
            }

            _sessionManager.AddSession(token, new UserResource("123", loginModel.Email, "Mo5", "Kareem"));

            return Ok(token);
        }

    }
}
