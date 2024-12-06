using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace FixEngine.Simulation
{
    public class Simulator              //The code needs cleaning
    {
        #region Fields
        private OrderService _orderService;
        private PositionService _positionsService;
        private RiskUserService _riskUserService;
        private PositionSimulation positionSimulation;
        private MarginSimulation marginSimulation;

        private readonly BufferBlock<Common.SymbolQuote> _quoteBuffer = new();

        public Channel<Position> PositionChannel { get; } = Channel.CreateUnbounded<Position>();
        public Channel<UserMargin> UserChannel { get; } = Channel.CreateUnbounded<UserMargin>();
        public Channel<Margin> MarginChannel { get; } = Channel.CreateUnbounded<Margin>();
        #endregion

        public Simulator(OrderService orderService, PositionService positionsService, RiskUserService riskUserService)
        {
            var incomingPrice = new ActionBlock<Common.SymbolQuote>(Simulation);
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            _quoteBuffer.LinkTo(incomingPrice, linkOptions);
            _orderService = orderService;
            _positionsService = positionsService;
            _riskUserService = riskUserService;
            positionSimulation = new PositionSimulation(orderService, positionsService, riskUserService);
            marginSimulation = new MarginSimulation(PositionChannel, UserChannel, MarginChannel);
        }

        #region Metods
        public async Task SaveNewPrice(Common.SymbolQuote quote)
        {
            await _quoteBuffer.SendAsync(quote);
            //await Simulation(quote);
        }

        public async Task<Position> ClosePosition(Position closePosition)
        => await positionSimulation.ClosePosition(closePosition); 

        public void NewOrderRequest(NewOrderRequestParameters newOrderRequest, RiskUser user)
        {
            //positionSimulation.ReceiveOrder(newOrderRequest);
            marginSimulation.ReceiveOrder(newOrderRequest, user);

            //var newOrder = new Order
            //{
            //    EntryPrice = newOrderRequest.TargetPrice,
            //    Status = newOrderRequest.TradeSide,
            //    GatewayType = GatewayType.BBook.ToString(),
            //    RiskUserId = newOrderRequest.RiskUserId
            //};

            //await _orderService.AddAsync(newOrder);
        }

        private async Task Simulation(Common.SymbolQuote quote)
        {
            //await positionSimulation.Simulation(quote, PositionChannel);
            //Console.WriteLine($"Simulate: {quote.SymbolName}");
            await marginSimulation.Simulation(quote);
        }
        #endregion
    }
}
