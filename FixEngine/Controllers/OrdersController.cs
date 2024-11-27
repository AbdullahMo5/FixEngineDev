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
        private readonly ApiService _apiService;

        public OrdersController(IOrderService orderService, IRiskUserService riskUserService, ApiService apiService)
        {
            _orderService = orderService;
            _riskUserService = riskUserService;
            _apiService = apiService;
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
                GatewayType = model.GatewayType,
                RiskUserId = model.RiskUserId,
                StopLoss = model.StopLoss,
                TakeProfit = model.TakeProfit,
            };

            var orderRequest = new NewOrderRequestParameters(model.Type, model.ClOrdId, model.SymbolId, model.SymbolName, model.TradeSide
                , model.Quantity, model.EntryPrice);

            var x = _riskUserService.GetGatewayType(3);
            var user = await _riskUserService.GetByIdAsync(model.RiskUserId);
            var client = _apiService.GetClient(token);
            if (client == null) return BadRequest("wrong Client");

            switch (x) {
                case GatewayType.ABook:
                    client.SendNewOrderRequest(orderRequest);
                    return Ok("Send to centroid");
                case GatewayType.BBook:
                    client.simulator.NewOrderRequest(orderRequest, user);
                    return Ok("Send to simulator");
            }

            if (await _orderService.AddAsync(order) > 0)
                return Ok(order);
            return BadRequest("Something went wrong");
        }

    }
}
