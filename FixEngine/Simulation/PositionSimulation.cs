using FixEngine.Entity;
using FixEngine.Services;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FixEngine.Simulation
{
    public class PositionSimulation
    {
        #region Fields
        private OrderService _orderService;
        private PositionService _positionsService;
        private RiskUserService _riskUserService;

        private readonly ConcurrentDictionary<int?, List<Position>> positionsBook = new ConcurrentDictionary<int?, List<Position>>();
        #endregion

        #region Properties
        public ConcurrentDictionary<int?, List<Position>> PositionsBook { get { return positionsBook; } }
        #endregion

        public PositionSimulation(OrderService orderService, PositionService positionsService, RiskUserService riskUserService)
        {
            _orderService = orderService;
            _positionsService = positionsService;
            _riskUserService = riskUserService;
        }

        #region Public Methods
        public async Task<Position> ClosePosition(Position closePosition)
        {
            var closPose = positionsBook[closePosition.SymbolId].FirstOrDefault(p => p.Id == closePosition.Id);

            //Close Pose
            closPose.TradeSide = closPose.TradeSide == "buy" ? "sell" : "buy";
            //Send Close Pose to database
            await _positionsService.AddAsync(closPose);
            //Add Profit to balance
            var riskUser = await _riskUserService.GetByIdAsync(closePosition.RiskUserId);
            if (riskUser == null) return null;
            riskUser.Balance += closePosition.Profit;
            await _riskUserService.Update(riskUser);
            //Remove Pose from Ram
            positionsBook[closePosition.SymbolId].Remove(closPose);

            return closPose;
        }

        public void ReceiveOrder(NewOrderRequestParameters newOrderRequest)
        {
            var posList = positionsBook.GetOrAdd(newOrderRequest.SymbolId, _ => new List<Position>());
            var newPos = new Position
            {
                EntryPrice = newOrderRequest.TargetPrice,
                SymbolName = newOrderRequest.SymbolName,
                SymbolId = newOrderRequest.SymbolId ?? 0,
                TradeSide = newOrderRequest.TradeSide,
                Volume = newOrderRequest.Quantity,
                RiskUserId = newOrderRequest.RiskUserId
            };

            lock (posList)
            {
                posList.Add(newPos);
            }
        }

        public async Task Simulation(Common.SymbolQuote quote, Channel<Position> positionChannel)
        {
            try
            {
                bool keyExict = positionsBook.ContainsKey(quote.SymbolId);
                if (!keyExict) return;

                foreach (var position in positionsBook[quote.SymbolId])
                {
                    decimal pNl;

                    if (position.TradeSide.ToLowerInvariant() == "buy")
                    {
                        pNl = (quote.Bid - position.EntryPrice) * position.Volume; //Check the logic
                    }
                    else
                    {
                        pNl = (position.EntryPrice - quote.Ask) * position.Volume; //Check the logic
                    }
                    position.Profit = pNl;

                    await positionChannel.Writer.WriteAsync(position);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Simulation: {ex.Message}");
            }
        }
        #endregion
    }
}
