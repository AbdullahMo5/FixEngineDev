using FixEngine.Entity;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiskUsersController : ControllerBase
    {
        private readonly IRiskUserService _riskUserService;

        public RiskUsersController(IRiskUserService riskUserService)
        {
            _riskUserService = riskUserService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => Ok(await _riskUserService.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Add(RiskUser riskUser)
        {
            if (await _riskUserService.AddAsync(riskUser) > 0)
                return Ok("RiskUser Added Success");
            return BadRequest("Something went wring");
        }
    }
}
