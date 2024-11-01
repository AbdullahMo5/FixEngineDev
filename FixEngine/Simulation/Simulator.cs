//using Common;
using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace FixEngine.Simulation
{
    public class Simulator
    {
        #region Fields
        private OrderService _orderService;
        private PositionsService _positionsService;

        private readonly ConcurrentDictionary<int?, List<Position>> positionsBook = new ConcurrentDictionary<int?, List<Position>>();

        private readonly BufferBlock<Common.SymbolQuote> _quoteBuffer = new();

        public Channel<Position> positionChannel { get; } = Channel.CreateUnbounded<Position>();
        public Channel<ExecutionReport> orderReportChannel { get; } = Channel.CreateUnbounded<ExecutionReport>();
        #endregion

        public Simulator(OrderService orderService, PositionsService positionsService)
        {
            var incomingPrice = new ActionBlock<Common.SymbolQuote>(Simulation);
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            _quoteBuffer.LinkTo(incomingPrice, linkOptions);
            _orderService = orderService;
            _positionsService = positionsService;
        }

        public async Task SaveNewPrice(Common.SymbolQuote quote)
        {
            await _quoteBuffer.SendAsync(quote);
        }
        public async Task<Position> ClosePosition(Position closePosition)
        {
            var closPose = positionsBook[closePosition.SymbolId].FirstOrDefault(p => p.Id == closePosition.Id);

            //Close Pose
            closPose.TradeSide  = closPose.TradeSide == "buy" ? "sell" : "buy";
            //Send Close Pose to database
            await _positionsService.AddAsync(closPose);
            //Remove Pose from Ram
            positionsBook[closePosition.SymbolId].Remove(closPose);

            return closPose;
        }
        public async Task ReceiveOrder(NewOrderRequestParameters newOrderRequest)
        {
            var posList = positionsBook.GetOrAdd(newOrderRequest.SymbolId, _ => new List<Entity.Position>());
            var newPos = new Position
            {
                EntryPrice = newOrderRequest.TargetPrice,
                SymbolName = newOrderRequest.SymbolName,
                SymbolId = newOrderRequest.SymbolId ?? 0,
                TradeSide = newOrderRequest.TradeSide,
                Volume = newOrderRequest.Quantity,
            };
            var newOrder = new Order
            {
                EntryPrice = newOrderRequest.TargetPrice,
                Status = newOrderRequest.TradeSide
            };

            await _orderService.AddAsync(newOrder);
            lock (posList)
            {
                posList.Add(newPos);
            }
        }

        private async Task Simulation(Common.SymbolQuote quote)
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
                        pNl = position.EntryPrice - quote.Ask * position.Volume; //Check the logic
                    }
                    else
                    {
                        pNl = position.EntryPrice - quote.Bid * position.Volume; //Check the logic
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
    }
}
