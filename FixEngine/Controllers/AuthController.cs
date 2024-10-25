using FixEngine.Enums;
using FixEngine.Models;
using FixEngine.Services;
using FixEngine.Shared;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _authService.Register(createUser);

            if (result == RegisterState.EmailAlreadyExist)
                return BadRequest(new CustomErrorResponse("Registration failed", new[] { $"{createUser.Email} is already exist" }));

            if (result == RegisterState.Failed)
                return BadRequest(new CustomErrorResponse("Registration failed", new[] { "Something went wrong" }));

            return Ok(new { data = "registration is successful" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestModel loginModel)
        {
            var token = await _authService.Login(loginModel.Email, loginModel.Password);

            if (token == null)
                return BadRequest(new CustomErrorResponse("Login failed", new[] { " Email Or Password incorrect" }));

            Console.WriteLine(_sessionManager.GetSession(token));

            return Ok(new { data = token });
        }

    }

    internal record CustomErrorResponse(string Message, string[] Errors);
}
