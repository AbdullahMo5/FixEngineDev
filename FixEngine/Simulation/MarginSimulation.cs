using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuickFix.FIX44;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace FixEngine.Simulation
{                       
    public class MarginSimulation                   //Clean Code
    {
        #region Fields
        private readonly CustomDictionary usersBook = new CustomDictionary();
        private Channel<Position> positionChannel;
        private Channel<UserMargin> userChannel;

        private readonly BufferBlock<UserMargin> _marginBuffer = new();
        #endregion

        #region Properties
        public CustomDictionary UserBook { get { return usersBook; } }
        #endregion

        public MarginSimulation(Channel<Position> positionChannel, Channel<UserMargin> userChannel)
        {
            this.positionChannel = positionChannel;
            this.userChannel = userChannel;
        }

        #region Public Method
        public void ReceiveOrder(NewOrderRequestParameters newOrderRequest, RiskUser user)
        {
            var userMargin = new UserMargin();

            userMargin.Balance = user.Balance;
            userMargin.Leverage = user.Leverage;
            userMargin.RiskUserId = newOrderRequest.RiskUserId;
            userMargin.PoseSize = newOrderRequest.Quantity;
            userMargin.UnFilledPositions.Add(PositionsHandler.NewUnFilledPosition(newOrderRequest));

            usersBook.AddOrUpdate(newOrderRequest.RiskUserId, newOrderRequest.SymbolId, userMargin);
            Console.WriteLine("Received Order!!");
        }
        public async Task Simulation(Common.SymbolQuote quote)
        {
            FillMarketPositions(quote);
            await CalculatePNL(quote);
        }

        private void FillMarketPositions(Common.SymbolQuote quote)
        {
            var users = usersBook.GetList(quote.SymbolId);
            if (users == null) return;

            foreach (var user in users)
            {
                if (user.UnFilledPositions.Count <= 0) continue;

                foreach (var position in user.UnFilledPositions)
                {
                    if (position.TradeSide == "buy")
                    {
                        position.EntryPrice = quote.Ask;
                    }
                    else { position.EntryPrice = quote.Bid; }

                    user.FilledPositions.Add(position);
                    Console.WriteLine($"Position with Symbol: {quote.SymbolName} Filled");
                }
                user.UnFilledPositions.Clear();
            }
        }

        private async Task CalculatePNL(Common.SymbolQuote quote)
        {
            var users = usersBook.GetList(quote.SymbolId);
            if (users == null) return;

            foreach (var user in users)
            {
                if (user.FilledPositions.Count <= 0) return;
                decimal pNl = 0;

                foreach (var position in user.FilledPositions)
                {
                    if (position.TradeSide.ToLowerInvariant() == "buy")
                    {
                        pNl += (quote.Bid - position.EntryPrice) * position.Volume; //Check the logic
                        position.Profit = (quote.Bid - position.EntryPrice) * position.Volume; //Check the logic
                    }
                    else
                    {
                        pNl += (position.EntryPrice - quote.Ask) * position.Volume; //Check the logic
                        position.Profit = (position.EntryPrice - quote.Ask) * position.Volume; //Check the logic
                    }
                    await positionChannel.Writer.WriteAsync(position);
                }
                var userMargin = usersBook.Get(user.RiskUserId);
                userMargin.PNL = pNl;
                //await userChannel.Writer.WriteAsync(userMargin);
            }
        }

        private async Task CalculateMarginLevel() { }
        #endregion
    }
}
