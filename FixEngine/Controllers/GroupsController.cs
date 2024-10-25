using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpPost]
        public async Task<IActionResult> Add(CreateGroupModel model)
        {
            if (await _groupService.IsGroupExistsAsync(model.Name))
                return BadRequest("Group Is Already Exist");
            var isGatewayExist = await _gatewayService.IsGatewayExistAsync(model.GatewayId);
            Group group = new() { Name = model.Name, GatewayId = model.GatewayId };
            if (await _groupService.AddAsync(group) > 0)
                return Ok("Group Added Success");
            return BadRequest("Something went wring");
        }
    }
}
