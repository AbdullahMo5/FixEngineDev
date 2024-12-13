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
        private GroupService _groupService;
        private PositionSimulation positionSimulation;
        private MarginSimulation marginSimulation;

        private readonly BufferBlock<Common.SymbolQuote> _quoteBuffer = new();

        public Channel<Position> PositionChannel { get; } = Channel.CreateUnbounded<Position>();
        public Channel<UserMargin> UserChannel { get; } = Channel.CreateUnbounded<UserMargin>();
        public Channel<Margin> MarginChannel { get; } = Channel.CreateUnbounded<Margin>();
        #endregion

        public Simulator(OrderService orderService, PositionService positionsService, RiskUserService riskUserService, GroupService groupService)
        {
            var incomingPrice = new ActionBlock<Common.SymbolQuote>(Simulation);
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            _quoteBuffer.LinkTo(incomingPrice, linkOptions);
            _orderService = orderService;
            _positionsService = positionsService;
            _riskUserService = riskUserService;
            positionSimulation = new PositionSimulation(orderService, positionsService, riskUserService);
            marginSimulation = new MarginSimulation(PositionChannel, UserChannel, MarginChannel);
            _groupService = groupService;
        }

        #region Metods
        public async Task SaveNewPrice(Common.SymbolQuote quote)
        {
            await _quoteBuffer.SendAsync(quote);
            //await UpdateDataTest(quote);
        }

        public async Task<Position> ClosePosition(string closePosition, int riskuserId, int symbolId)
        {
           return marginSimulation.ClosePosition(closePosition, riskuserId, symbolId);
        }

        public void NewOrderRequest(NewOrderRequestParameters newOrderRequest, RiskUser user, Group group)
        {
            marginSimulation.ReceiveOrder(newOrderRequest, user, group);
        }

        private async Task Simulation(Common.SymbolQuote quote)
        {
            await marginSimulation.Simulation(quote);
        }

        private async Task UpdateDataTest(Common.SymbolQuote quote)
        {
            var users = marginSimulation.UserBook.GetList(quote.SymbolId);
            if (users == null) return;
            foreach (var user in users)
            {
                var DBUser = await _riskUserService.GetByIdAsync(user.RiskUserId);
                var DBGroup = await _groupService.GetByIdAsync(user.GroupId);
                if(DBUser == null || DBGroup == null) continue;

                marginSimulation.UpdateData(user.RiskUserId, quote.SymbolId, DBUser, DBGroup);
            }
        }
        #endregion
    }
}
