using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Services;
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
        private Channel<Margin> marginChannel;

        private readonly BufferBlock<UserMargin> _marginBuffer = new();
        #endregion

        #region Properties
        public CustomDictionary UserBook { get { return usersBook; } }
        #endregion

        public MarginSimulation(Channel<Position> positionChannel, Channel<UserMargin> userChannel, Channel<Margin> marginChannel)
        {
            this.positionChannel = positionChannel;
            this.marginChannel = marginChannel;

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

        public Position ClosePosition(string positionId, int riskUserId)
        {
            var user = usersBook.Get(riskUserId);
            var position = PositionsHandler.ClosePosition(user.FilledPositions, positionId);

            return position;
        }

        public void Liquidation()
        {

        }
        #endregion

        #region Private Methods
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
                decimal TpNl = 0;
                decimal Tlot = 0;

                foreach (var position in user.FilledPositions)
                {
                    decimal pNl = 0;

                    if (position.TradeSide.ToLowerInvariant() == "buy")
                    {
                        pNl = (quote.Bid - position.EntryPrice) * position.Volume * quote.ContractSize; //Check the logic
                        TpNl += pNl;
                        position.Profit = pNl;
                        position.ClosePrice = quote.Ask;
                    }
                    else
                    {
                        pNl = (position.EntryPrice - quote.Ask) * position.Volume * quote.ContractSize; //Check the logic
                        TpNl += pNl;
                        position.Profit = pNl;
                        position.ClosePrice = quote.Bid;
                    }
                    Tlot += position.Volume;
                    await positionChannel.Writer.WriteAsync(position);
                }
                decimal usedMargin = (Tlot * quote.ContractSize) / user.Leverage;
                var margin = usersBook.Get(user.RiskUserId);

                if (margin.SymboolBook.ContainsKey(quote.SymbolId)) { 
                    margin.SymboolBook[quote.SymbolId].PnL = TpNl;
                    margin.SymboolBook[quote.SymbolId].UsedMargin = usedMargin;
                }
                else { margin.SymboolBook.Add(quote.SymbolId, new() { PnL = TpNl, UsedMargin = usedMargin }); }
                await CalculateMarginLevel(user.RiskUserId);
            }
        }

        private async Task CalculateMarginLevel(int riskUserId)
        {
            if (!usersBook.ContainsKey(riskUserId)) return;
            var user = usersBook.Get(riskUserId);

            decimal equity = user.Balance + TotalPnL(riskUserId);
            decimal marginLevel = (equity / TotalUsedmargin(riskUserId)) * 100;

            Margin margin = new Margin { RiskUserId = riskUserId, Equity = equity, MarginLevel = marginLevel, PNL = user.PNL };

            await marginChannel.Writer.WriteAsync(margin);

            Console.WriteLine($"RiskUserID: {riskUserId} MarginLevel: {marginLevel} Equity: {equity}");
        }

        private decimal TotalPnL(int riskUserId)
        {
            decimal tPnL = 0;

            if (!usersBook.ContainsKey(riskUserId)) return tPnL;
            var user = usersBook.Get(riskUserId);
            foreach (var item in user.SymboolBook)
            {
                tPnL += item.Value.PnL;
            }

            return tPnL;
        }
        private decimal TotalUsedmargin(int riskUserId)
        {
            decimal tLot = 0;

            if (!usersBook.ContainsKey(riskUserId)) return tLot;
            var user = usersBook.Get(riskUserId);
            foreach (var item in user.SymboolBook)
            {
                tLot += item.Value.UsedMargin;
            }

            return tLot;
        }
        #endregion
    }
}
