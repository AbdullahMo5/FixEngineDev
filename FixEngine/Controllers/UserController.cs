using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FixEngine.Controllers
{
    [Route("api/")]
    [ApiController]
    public class UserController:ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private UserService _service;

        public UserController(ILogger<UserController> logger, UserService service)
        {
            _logger = logger;
            _service = service;
        }
        [HttpPost("Create")]
        public async Task<ActionResult<string>> CreateUser([FromBody] CreateUserRequestModel user)
        {
            //TODO validation
            bool status = _service.AddUser(user);
            if (!status)
            {
                return BadRequest("User already exists");
            }
            return Ok("User created sucessfully");
        }
        [HttpGet("GetUser")]
        public async Task<ActionResult<string>> GetUser([FromQuery] string email)
        {
            var user = _service.FetchUserByEmail(email);
            if (user != null)
                return Ok(user);
            else return NotFound("User not found");
        }
    }
}
