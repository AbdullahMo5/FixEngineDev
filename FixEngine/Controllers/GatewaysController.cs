using FixEngine.Entity;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewaysController : ControllerBase
    {
        private readonly IGatewayService _gatewayService;

        public GatewaysController(IGatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => Ok(await _gatewayService.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Add(Gateway gateway)
        {
            if (await _gatewayService.AddAsync(gateway) > 0)
                return Ok("Gateway Added Success");
            return BadRequest("Something went wring");
        }
    }
}
