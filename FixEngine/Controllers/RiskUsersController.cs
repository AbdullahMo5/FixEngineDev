using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RiskUsersController : ControllerBase
    {
        private readonly IRiskUserService _riskUserService;
        private readonly IPasswordHasher<RiskUser> _passwordHasher;
        private readonly IGroupService _groupService;

        public RiskUsersController(IRiskUserService riskUserService, IPasswordHasher<RiskUser> passwordHasher, IGroupService groupService)
        {
            _riskUserService = riskUserService;
            _passwordHasher = passwordHasher;
            _groupService = groupService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => Ok(await _riskUserService.GetAllAsync());

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var riskUser = await _riskUserService.GetByIdAsync(id);
            if (riskUser is null)
                return NotFound("There is no User with this id");
            return Ok(riskUser);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateRiskUserModel model)
        {
            if (await _riskUserService.IsExist(e => e.Email.ToLower() == model.Email.ToLower()))
                return BadRequest($"{model.Email} already Exist");
            if (!await _groupService.IsExist(e => e.Id == model.GroupId))
                return BadRequest("There is no Group with this id");
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var newRiskUser = new RiskUser
            {
                Email = model.Email,
                AppUserId = userId,
                Balance = model.Balance,
                GroupId = model.GroupId,
                Name = model.Name,
                Leverage = model.Leverage,
            };
            newRiskUser.IP = HttpContext.Connection.RemoteIpAddress?.ToString();

            newRiskUser.Password = _passwordHasher.HashPassword(newRiskUser, model.Password);

            if (await _riskUserService.AddAsync(newRiskUser) > 0)
                return Ok("RiskUser Added Success");
            return BadRequest("Something went wring");
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, UpdateRiskUserModel model)
        {
            if (id != model.Id)
                return BadRequest("Id is not the same as in body");
            var riskFromDb = await _riskUserService.GetByIdAsync(id);
            if (riskFromDb is null)
                return BadRequest("There is no riskUser With this id");
            riskFromDb.IP = model.IP;
            riskFromDb.Email = model.Email;
            riskFromDb.Balance = model.Balance;
            riskFromDb.Name = model.Name;
            riskFromDb.GroupId = model.GroupId;
            riskFromDb.Leverage = model.Leverage;
            riskFromDb.Password = _passwordHasher.HashPassword(riskFromDb, model.Password);
            if (await _riskUserService.Update(riskFromDb) > 0)
                return Ok("updated success");
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var riskUser = await _riskUserService.GetByIdAsync(id);
            if (riskUser is null)
                return BadRequest("There is no riskuser with this id");
            if (await _riskUserService.Delete(riskUser) > 0)
                return Ok("Deleted success");
            return BadRequest("Something went wrong");
        }
    }
}
