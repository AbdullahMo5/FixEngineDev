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
        private readonly IGroupService _groupService;
        private readonly ApiService _apiService;

        public OrdersController(IOrderService orderService, IRiskUserService riskUserService, IGroupService groupService, ApiService apiService)
        {
            _orderService = orderService;
            _riskUserService = riskUserService;
            _apiService = apiService;
            _groupService = groupService;
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
        public async Task<IActionResult> Add(string token, CreateOrderModel model)
        {
            var order = new Order
            {
                ClosePrice = model.ClosePrice,
                CloseTime = DateTime.UtcNow,
                EntryPrice = model.EntryPrice,
                FinalLoss = model.FinalLoss,
                FinalProfit = model.FinalProfit,
                RiskUserId = model.RiskUserId,
                StopLoss = model.StopLoss,
                TakeProfit = model.TakeProfit,
            };

            var orderRequest = new NewOrderRequestParameters(model.Type, model.ClOrdId, model.SymbolId, model.SymbolName, model.TradeSide
                , model.Quantity, model.EntryPrice);

            var user = await _riskUserService.GetByIdAsync(model.RiskUserId);
            if (user == null) return BadRequest("wrong User");
            var group = await _groupService.GetByIdAsync(user.GroupId);
            if (group == null) return BadRequest("wrong Group");
            var x = _riskUserService.GetGatewayType(user.GroupId);
            var client = _apiService.GetClient(token);                            //For Test purpose only
            if (client == null) return BadRequest("wrong Client");

            switch (x)
            {
                case GatewayType.ABook:
                    client.SendNewOrderRequest(orderRequest);
                    return Ok("Send to centroid");
                case GatewayType.BBook:
                    client.simulator.NewOrderRequest(orderRequest, user, group);
                    return Ok("Send to simulator");
            }

            if (await _orderService.AddAsync(order) > 0) return Ok(order);

            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order is null)
                return BadRequest("There is no order with this id");
            if (await _orderService.Delete(order) > 0)
                return Ok("Deleted success");
            return BadRequest("Something went wrong");
        }
    }
}
