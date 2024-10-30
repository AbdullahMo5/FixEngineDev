using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IRiskUserService _riskUserService;

        public OrdersController(IOrderService orderService, IRiskUserService riskUserService)
        {
            _orderService = orderService;
            _riskUserService = riskUserService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => Ok(await _orderService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return BadRequest("There is no Order With this id");
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateOrderModel model)
        {
            var x = _riskUserService.GetGatewayType(3);
            if (x == GatewayType.ABook)
                return Ok("Send to centroid");
            var order = new Order
            {
                ClosePrice = model.ClosePrice,
                CloseTime = DateTime.UtcNow,
                EntryPrice = model.EntryPrice,
                FinalLoss = model.FinalLoss,
                FinalProfit = model.FinalProfit,
                GatewayType = model.GatewayType,
                RiskUserId = 3,
                Status = model.Status,
                StopLoss = model.StopLoss,
                TakeProfit = model.TakeProfit,
            };

            if (await _orderService.AddAsync(order) > 0)
                return Ok(order);
            return BadRequest("Something went wrong");
        }

    }
}
