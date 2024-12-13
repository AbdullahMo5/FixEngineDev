using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IGatewayService _gatewayService;

        public GroupsController(IGroupService groupService, IGatewayService gatewayService)
        {
            _groupService = groupService;
            _gatewayService = gatewayService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
          => Ok(await _groupService.GetAllAsync());

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var gateWay = await _groupService.GetByIdAsync(id);
            if (gateWay is null)
                return NotFound("There is no Group with this id");
            return Ok(gateWay);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateGroupModel model)
        {
            if (await _groupService.IsExist(e => e.Name.ToLower() == model.Name.ToLower()))
                return BadRequest("Group Is Already Exist");
            var isGatewayExist = await _gatewayService.IsExist(e => e.Id == model.GatewayId);
            if (!isGatewayExist)
                return BadRequest("There is no gateway with this id");
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            Group group = new() { Name = model.Name, GatewayId = model.GatewayId, CreatedBy = userId };
            if (await _groupService.AddAsync(group) > 0)
                return Ok("Group Added Success");
            return BadRequest("Something went wring");
        }

        [HttpPut]
        public async Task<IActionResult> Update(int groupId, CreateGroupModel group)
        {
            var isGatewayExist = await _gatewayService.IsExist(e => e.Id == group.GatewayId);
            if (!isGatewayExist)
                return BadRequest("There is no gateway with this id");

            var groupFromDb = await _groupService.GetByIdAsync(groupId);
            if (groupFromDb is null)
                return BadRequest("There is no gateway with this id");

            groupFromDb.Name = group.Name;
            groupFromDb.GatewayId = group.GatewayId;
            groupFromDb.MarginCall = group.MarginCall;
            groupFromDb.StopOut = group.StopOut;
            if (await _groupService.Update(groupFromDb) > 0)
                return Ok("Updated success");

            return BadRequest("Something went wrong");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _groupService.GetByIdAsync(id);
            if (group is null)
                return BadRequest("There is no group with this id");
            if (await _groupService.Delete(group) > 0)
                return Ok("Deleted successfully");
            return BadRequest("Something went wrong");
        }
    }
}
