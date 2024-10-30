using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var gateWay = await _gatewayService.GetByIdAsync(id);
            if (gateWay is null)
                return NotFound("There is no Gateway with this id");
            return Ok(gateWay);
        }


        [HttpPost]
        public async Task<IActionResult> Add(CreateGateWayModel model)
        {
            if (await _gatewayService.IsExist(e => e.Name.ToLower() == model.Name.ToLower()))
                return BadRequest("Gateway Already Exist");
            Gateway gateway = new()
            {
                Name = model.Name,
                Type = model.Type,
            };
            if (await _gatewayService.AddAsync(gateway) > 0)
                return Ok("Gateway Added Success");
            return BadRequest("Something went wring");
        }

        [HttpPut]
        public async Task<IActionResult> Update(int gatewayId, Gateway gateway)
        {
            if (gateway.Id != gatewayId)
                return BadRequest("Id in paremeters does not match id in the body");

            var gatewayFromDb = await _gatewayService.GetByIdAsync(gatewayId);
            if (gatewayFromDb is null)
                return BadRequest("There is no gateway with this id");

            if (gatewayFromDb.Name != gateway.Name && await _gatewayService.IsExist(e => e.Name.ToLower() == gateway.Name.ToLower()))
                return BadRequest("This name is already taken");

            gatewayFromDb.Name = gateway.Name;
            gatewayFromDb.Type = gateway.Type;
            if (await _gatewayService.Update(gatewayFromDb) > 0)
                return Ok("Updated success");

            return BadRequest("Something went wrong");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var gateway = await _gatewayService.GetByIdAsync(id);
            if (gateway is null)
                return BadRequest("There is no gateway with this id");
            if (await _gatewayService.Delete(gateway) > 0)
                return Ok("Deleted successfully");
            return BadRequest("Something went wrong");
        }
    }
}
