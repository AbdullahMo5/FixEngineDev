using Common;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Transport;
using System.Globalization;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace FixEngine.Services
{
    public class FixClient : IDisposable
    {
        private readonly SocketInitiator _quoteInitiator;
        private readonly SocketInitiator _tradeInitiator;

        public readonly QuickFix44App _quoteApp;
        private readonly QuickFix44App _tradeApp;

        private static int positionsCount = -1;
        private Common.Symbol[] _symbols;
        private string _account;
        private string _lp;

        private ApiCredentials _credentials;
        private SymbolService _symbolService;
        public FixClient(ApiCredentials credentials, string lp, SymbolService symbolService)
        {
            _credentials = credentials;
            _account = credentials.Account;
            _symbolService = symbolService;
            _lp = lp;
            _tradeApp = new(credentials.TradeUsername, credentials.TradePassword, credentials.TradeSenderCompId, credentials.TradeSenderSubId, credentials.TradeTargetCompId);
            _quoteApp = new(credentials.QuoteUsername, credentials.QuotePassword, credentials.QuoteSenderCompId, credentials.QuoteSenderSubId, credentials.QuoteTargetCompId);

            var incomingMessagesProcessingBlock = new ActionBlock<QuickFix.Message>(ProcessIncomingMessage);
            var outgoingMessagesProcessingBlock = new ActionBlock<QuickFix.Message>(ProcessOutgoingMessage);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            _tradeApp.IncomingMessagesBuffer.LinkTo(incomingMessagesProcessingBlock, linkOptions);
            _quoteApp.IncomingMessagesBuffer.LinkTo(incomingMessagesProcessingBlock, linkOptions);

            _tradeApp.OutgoingMessagesBuffer.LinkTo(outgoingMessagesProcessingBlock, linkOptions);
            _quoteApp.OutgoingMessagesBuffer.LinkTo(outgoingMessagesProcessingBlock, linkOptions);

            var tradeSettings = SessionSettingsFactory.GetSessionSettings(lp, credentials.TradeHost, credentials.TradePort, credentials.TradeSenderCompId, credentials.TradeSenderSubId, credentials.TradeTargetSubId, credentials.TradeTargetCompId, credentials.TradeResetOnLogin, credentials.TradeSsl);
            var quoteSettings = SessionSettingsFactory.GetSessionSettings(lp, credentials.QuoteHost, credentials.QuotePort, credentials.QuoteSenderCompId, credentials.QuoteSenderSubId, credentials.QuoteTargetSubId, credentials.QuoteTargetCompId, credentials.QuoteResetOnLogin, credentials.QuoteSsl);
            Console.WriteLine("Hi From line 49");
            Console.WriteLine($"{lp} | {credentials.QuoteHost} | {credentials.QuotePort} | {credentials.QuoteSenderCompId} | {credentials.QuoteSenderSubId} | {credentials.QuoteTargetSubId} | {credentials.QuoteTargetCompId} | {credentials.QuoteResetOnLogin} | {credentials.QuoteSsl}");
            var tradeStoreFactory = new FileStoreFactory(tradeSettings);
            var tradeLogFactory = new QuickFix.FileLogFactory(tradeSettings);
            var quoteStoreFactory = new FileStoreFactory(quoteSettings);

            var quoteLogFactory = new QuickFix.FileLogFactory(quoteSettings);

            _tradeInitiator = new(_tradeApp, tradeStoreFactory, tradeSettings, tradeLogFactory);
            _quoteInitiator = new(_quoteApp, quoteStoreFactory, quoteSettings, quoteLogFactory);
        }

        public Channel<Log> LogsChannel { get; } = Channel.CreateUnbounded<Log>();
        public Channel<Log> TradeLogsChannel { get; } = Channel.CreateUnbounded<Log>();

        public Channel<ExecutionReport> ExecutionReportChannel { get; } = Channel.CreateUnbounded<ExecutionReport>();
        public Channel<ExecutionReport> OrdersExecutionReportChannel { get; } = Channel.CreateUnbounded<ExecutionReport>();

        public Channel<Position> PositionReportChannel { get; } = Channel.CreateUnbounded<Position>();
        public Channel<SymbolQuote> MarketDataSnapshotFullRefreshChannel { get; } = Channel.CreateUnbounded<SymbolQuote>();

        public Channel<Common.Symbol> SecurityChannel { get; } = Channel.CreateUnbounded<Common.Symbol>();

        public void Connect()
        {
            try
            {
                Console.WriteLine($"Starting trade session on url: {_credentials.TradeHost} port: {_credentials.TradePort}");
                _tradeInitiator.Start();
                Console.WriteLine($"Starting market data session on url: {_credentials.QuoteHost} port: {_credentials.QuotePort}");
                _quoteInitiator.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _tradeInitiator?.Stop();
                _quoteInitiator?.Stop();
                throw ex;
            }
        }

        public void Dispose()
        {
            _tradeApp.Dispose();
            _quoteApp.Dispose();
            _tradeInitiator?.Dispose();
            _quoteInitiator?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task ProcessOutgoingMessage(QuickFix.Message message)
        {
            if (message is QuickFix.FIX44.MarketDataRequest) return;

            await LogsChannel.Writer.WriteAsync(new("Sent", GetMessageTypeString(message), DateTimeOffset.UtcNow, message.ToString('|')));
        }
        private string GetMessageTypeString(QuickFix.Message message)
        {
            string type = "";
            switch (message)
            {
                case QuickFix.FIX44.Heartbeat:
                    type = "HEARTBEAT";
                    break;
                case QuickFix.FIX44.Logon:
                    type = "LOGIN";
                    break;
                case QuickFix.FIX44.Logout:
                    type = "LOGOUT";
                    break;
                case QuickFix.FIX44.SecurityListRequest:
                    type = "SECURITY LIST REQUEST";
                    break;
                case QuickFix.FIX44.SecurityList:
                    type = "SECURITY LIST";
                    break;
                case QuickFix.FIX44.MarketDataRequest:
                    type = "MARKET DATA REQUEST";
                    break;
                case QuickFix.FIX44.MarketDataRequestReject:
                    type = "MARKET DATA REQUEST REJECT";
                    break;
                case QuickFix.FIX44.MarketDataSnapshotFullRefresh:
                    type = "MARKET DATA";
                    break;
                case QuickFix.FIX44.OrderMassStatusRequest:
                    type = "ORDER STATUS REQUEST";
                    break;
                case QuickFix.FIX44.RequestForPositions:
                    type = "POSITION REQUEST";
                    break;
                case QuickFix.FIX44.PositionReport:
                    type = "POSITION REPORT";
                    break;
                case QuickFix.FIX44.ExecutionReport:
                    type = "EXECUTION REPORT";
                    break;
                case QuickFix.FIX44.BusinessMessageReject:
                    type = "BUSINESS REJECT";
                    break;
                case QuickFix.FIX44.NewOrderSingle:
                    type = "NEW ORDER REQUEST";
                    break;
                case QuickFix.FIX44.Reject:
                    type = "REJECT";
                    break;
                case QuickFix.FIX44.ResendRequest:
                    type = "RESEND REQUEST";
                    break;
                case QuickFix.FIX44.SequenceReset:
                    type = "SEQUENCE RESET";
                    break;
                case QuickFix.FIX44.TestRequest:
                    type = "TEST REQUEST";
                    break;
            }
            return type;
        }

        private async Task ProcessIncomingMessage(QuickFix.Message message)
        {
            if (message is not QuickFix.FIX44.MarketDataSnapshotFullRefresh)
            {
                await LogsChannel.Writer.WriteAsync(new("Received", GetMessageTypeString(message), DateTimeOffset.UtcNow, message.ToString('|')));
            }

            if (message is QuickFix.FIX44.Logon)
            {
                Console.WriteLine("Client logged in ...");
                if (_lp.Equals("CTRADER", StringComparison.OrdinalIgnoreCase) && message.Header.IsSetField(50) && message.Header.GetString(50).Equals("TRADE", StringComparison.OrdinalIgnoreCase) && _tradeInitiator.IsLoggedOn)
                {
                    SendSecurityListRequest();
                }
                else if (_lp.Equals("CENTROID", StringComparison.OrdinalIgnoreCase) && message.Header.IsSetField(56) && message.Header.GetString(56).Equals("MD_Fintic-FIX-TEST", StringComparison.OrdinalIgnoreCase))
                {
                    await OnSecurityList();
                }
                return;
            }

            switch (message)
            {
                case QuickFix.FIX44.SecurityList securityList:
                    await OnSecurityList(securityList);
                    break;

                case QuickFix.FIX44.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh:
                    await OnMarketDataSnapshotFullRefresh(marketDataSnapshotFullRefresh);
                    break;

                case QuickFix.FIX44.PositionReport positionReport:
                    await OnPositionReport(positionReport);
                    break;

                case QuickFix.FIX44.ExecutionReport executionReport:
                    await OnExecutionReport(executionReport);
                    break;
                case QuickFix.FIX44.BusinessMessageReject reject:
                    await OnBusinessReject(reject);
                    break;
            }
        }

        private async Task OnBusinessReject(QuickFix.FIX44.BusinessMessageReject reject)
        {
            Console.WriteLine("Recvd business reject =>");
            Console.WriteLine(reject.ToString());
        }
        private async Task OnExecutionReport(QuickFix.FIX44.ExecutionReport executionReport)
        {
            var order = (_lp.Equals("CTRADER", StringComparison.OrdinalIgnoreCase))
                        ? executionReport.GetOrder()
                        : (_lp.Equals("CENTROID", StringComparison.OrdinalIgnoreCase))
                            ? executionReport.GetOrderII()
                            : executionReport.GetOrderII();

            string side = ((char)executionReport.Side.getValue() == Side.BUY)
                ? "BUY"
                : ((char)executionReport.Side.getValue() == Side.SELL)
                    ? "SELL"
                    : executionReport.Side.getValue().ToString();

            Console.WriteLine("Recv execution: ", order);

            if (_lp.Equals("CTRADER", StringComparison.OrdinalIgnoreCase)) order.SymbolName = _symbols.FirstOrDefault(symbol => symbol.Id == order.SymbolId)?.Name;

            if (order.Type.Equals("Market", StringComparison.OrdinalIgnoreCase) && executionReport.CumQty.getValue() > 0)
            {
                //await PositionReportChannel.Writer.WriteAsync(null);
                char executionType = executionReport.ExecType.getValue();
                string clOrderId = executionReport.ClOrdID.getValue();
                string execId = executionReport.IsSetField(17) ? executionReport.GetString(17) : string.Empty;
                await ExecutionReportChannel.Writer.WriteAsync(new(execId, executionType, order, clOrderId));
                Console.WriteLine("Requesting for positions");
                await TradeLogsChannel.Writer.WriteAsync(new("IN", "ExecutionReport", DateTimeOffset.UtcNow, $"(Execution: Market) Account: {executionReport.Account}: #{order.PositionId}    {order.SymbolName}     {order.Volume}      {side}      {order.Price}  - OrdStatus: {order.OrderStatus} - ExecutedQTY: {executionReport.CumQty} - RemainingQTY: {executionReport.LeavesQty} "));
                SendPositionsRequest();
            }
            //pending orders
            else if (order.Type.Equals("Market", StringComparison.OrdinalIgnoreCase) is false)
            {
                char executionType = executionReport.ExecType.getValue();
                string clOrderId = executionReport.ClOrdID.getValue();
                string execId = executionReport.IsSetField(17) ? executionReport.GetString(17) : string.Empty;

                //if (executionType != '4' && executionType != '8' && executionType != 'C' && executionType != 'F')
                await OrdersExecutionReportChannel.Writer.WriteAsync(new(execId, executionType, order, clOrderId));
                await TradeLogsChannel.Writer.WriteAsync(new("IN", "ExecutionReport", DateTimeOffset.UtcNow, $"(Execution: {order.Type}) - Account: {executionReport.Account} - OrderID: {executionReport.OrderID} - OrdStatus: {executionReport.OrdStatus} - Symbol: {executionReport.Symbol} - OrdQTY: {executionReport.OrderQty} - ExecutedQTY: {executionReport.CumQty} - RemainingQTY: {executionReport.LeavesQty} - Direction: {side} - OrdType: {order.Type} - Price: {executionReport?.Price} "));
            }
        }

        private async Task OnPositionReport(QuickFix.FIX44.PositionReport positionReport)
        {
            Console.WriteLine("Received positions");
            Console.WriteLine(positionReport.ToString());
            if (positionReport.IsSetField(728) && positionReport.GetInt(728) == 2)
            {
                Console.WriteLine("No open positions");
                Position position1 = new Position()
                {
                    Index = -1,
                    IsEmpty = true
                };
                await PositionReportChannel.Writer.WriteAsync(position1);
            }
            if (positionReport.TotalNumPosReports.getValue() == 0) return;

            var position = positionReport.GetPosition();
            if (position == null) return;

            if (_symbols is not null)
            {
                position.SymbolName = _symbols.FirstOrDefault(symbol => symbol.Id == position.SymbolId)?.Name;
            }
            position.Index = positionsCount + 1;
            positionsCount++;
            position.IsEmpty = false;
            Console.WriteLine("Positions Count = > " + positionsCount);
            await PositionReportChannel.Writer.WriteAsync(position);
        }

        private async Task OnMarketDataSnapshotFullRefresh(QuickFix.FIX44.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {
            if (_lp.Equals("CTRADER", StringComparison.OrdinalIgnoreCase))
            {
                var symbolQuote = marketDataSnapshotFullRefresh.GetSymbolQuote();
                var symbol = _symbols.DefaultIfEmpty(null).First(i => i.Id == symbolQuote.SymbolId);
                SymbolQuote quote = new SymbolQuote(symbolQuote.SymbolId, symbol.Name, symbolQuote.Bid, symbolQuote.Ask, symbol.Digits);
                if (symbol != null)
                    await MarketDataSnapshotFullRefreshChannel.Writer.WriteAsync(quote);
            }
            else if (_lp.Equals("CENTROID", StringComparison.OrdinalIgnoreCase))
            {
                var symbolQuote = marketDataSnapshotFullRefresh.GetSymbolQuoteII();
                var symbol = _symbolService.GetSymbolByLP(_lp, symbolQuote.SymbolName);

                if (symbol != null)
                {
                    SymbolQuote quote = new SymbolQuote(Int32.Parse(symbol.Id), symbol.Name, symbolQuote.Bid, symbolQuote.Ask, symbol.Digits);
                    await MarketDataSnapshotFullRefreshChannel.Writer.WriteAsync(quote);
                }
            }


        }

        private async Task OnSecurityList(QuickFix.FIX44.SecurityList securityList)
        {
            _symbols = securityList.GetSymbols().OrderBy(symbol => symbol.Id).ToArray();

            SendPositionsRequest();
            SendOrderMassStatusRequest();
            int limit = _symbols.Length > 100 ? 100 : _symbols.Length;
            for (int i = 0; i < limit; i++)
            {

                await SecurityChannel.Writer.WriteAsync(_symbols[i]);
                SendMarketDataRequest(true, _symbols[i].Id);
            }
            /*foreach (var symbol in _symbols)
            {
                await SecurityChannel.Writer.WriteAsync(symbol);

                SendMarketDataRequest(true, symbol.Id);
            }*/

            SecurityChannel.Writer.TryComplete();
        }
        private async Task OnSecurityList()
        {
            var symbols = _symbolService.GetSymbols();
            foreach (var symbol in symbols)
            {
                var _ssymbol = new Common.Symbol(Int32.Parse(symbol.Id), symbol.LPSymbolName, symbol.Digits);
                await SecurityChannel.Writer.WriteAsync(new Common.Symbol(Int32.Parse(symbol.Id), symbol.LPSymbolName, symbol.Digits));
                SendMarketDataRequest(true, symbol.LPSymbolName);
            }
            SecurityChannel.Writer.TryComplete();
        }
        public async void SendNewOrderRequest(NewOrderRequestParameters parameters)
        {
            Console.WriteLine("Recvd new order send request. Sending order");
            var ordType = new OrdType(parameters.Type.ToLowerInvariant() switch
            {
                "market" => OrdType.MARKET,
                "limit" => OrdType.LIMIT,
                "stop" => OrdType.STOP,
                _ => throw new Exception("unsupported input"),
            });

            var message = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(parameters.ClOrdId),
                new QuickFix.Fields.Symbol(parameters.SymbolName ?? parameters.SymbolId?.ToString(CultureInfo.InvariantCulture)),
                new Side(parameters.TradeSide.ToLowerInvariant().Equals("buy", StringComparison.OrdinalIgnoreCase) ? '1' : '2'),
                new TransactTime(DateTime.Now),
                ordType);
            if (!string.IsNullOrWhiteSpace(_account)) message.Set(new Account(_account));
            message.Set(new OrderQty(Convert.ToDecimal(parameters.Quantity)));

            if (ordType.getValue() != OrdType.MARKET)
            {
                message.Set(new TimeInForce('1'));

                if (parameters.TargetPrice > 0)
                {
                    if (ordType.getValue() == OrdType.LIMIT)
                    {
                        message.Set(new Price(Convert.ToDecimal(parameters.TargetPrice)));
                    }
                    else
                    {
                        if (_lp.Equals("CENTROID", StringComparison.OrdinalIgnoreCase)) message.Set(new Price(Convert.ToDecimal(parameters.TargetPrice)));
                        else if (_lp.Equals("CTRADER", StringComparison.OrdinalIgnoreCase)) message.Set(new StopPx(Convert.ToDecimal(parameters.TargetPrice)));
                    }
                }

                if (parameters.Expiry.HasValue && _lp.Equals("CTRADER", StringComparison.OrdinalIgnoreCase))
                {
                    message.Set(new ExpireTime(parameters.Expiry.Value));
                }
            }
            else
            {
                message.Set(new TimeInForce(TimeInForce.FILL_OR_KILL));

                if (parameters.PositionId.HasValue)
                {
                    message.SetField(new StringField(721, parameters.PositionId.Value.ToString(CultureInfo.InvariantCulture)));
                }
            }

            if (string.IsNullOrWhiteSpace(parameters.Designation) is false)
            {
                message.Set(new Designation(parameters.Designation));
            }

            message.Header.GetString(Tags.BeginString);

            _tradeApp.SendMessage(message);
            await TradeLogsChannel.Writer.WriteAsync(new("OUT", "NewOrderSingleRequest", DateTimeOffset.UtcNow, $"New Order - Symbol: {parameters.SymbolName} - QTY: {parameters.Quantity} - Direction: {parameters.TradeSide} - OrdType: {parameters.Type} - Price: {parameters?.TargetPrice} - Expiry: {parameters?.Expiry}"));
        }
        public async void SendOrderAmmendRequest(OrderAmmendRequest parameters)
        {
            Console.WriteLine("Recvd order ammend request. Sending order");
            var ordType = new OrdType(parameters.OrderType.ToLowerInvariant() switch
            {
                "market" => OrdType.MARKET,
                "limit" => OrdType.LIMIT,
                "stop" => OrdType.STOP,
                _ => throw new Exception("unsupported input"),
            });
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            var message = new QuickFix.FIX44.OrderCancelReplaceRequest();
            Console.WriteLine("OrigCLOrdID => ", parameters.ClOrderId);
            message.Set(new OrigClOrdID(parameters.ClOrderId));
            message.Set(new ClOrdID(unixTime.ToString()));
            message.Set(new OrderID(parameters.OrderId));
            message.Set(new OrderQty(parameters.OrderQty));
            if (ordType.getValue() == OrdType.LIMIT)
            {
                message.Set(new Price(Convert.ToDecimal(parameters.TargetPrice)));
            }
            else
            {
                message.Set(new StopPx(Convert.ToDecimal(parameters.TargetPrice)));
            }
            if (parameters.Expiry.HasValue)
            {
                message.Set(new ExpireTime(parameters.Expiry.Value));
            }

            message.Header.GetString(Tags.BeginString);

            _tradeApp.SendMessage(message);

            await TradeLogsChannel.Writer.WriteAsync(new("OUT", "OrderAmmendRequest", DateTimeOffset.UtcNow, $"Ammend - OrderID: {parameters.OrderId}# - QTY: {parameters.OrderQty} - OrdType: {parameters.OrderType} - TargetPrice: {parameters?.TargetPrice} - Expiry: {parameters?.Expiry}"));
        }

        public async void SendOrderCancelRequest(OrderCancelRequestParameters requestParams)
        {
            var message = new OrderCancelRequest();
            message.Set(new OrigClOrdID(requestParams.OrigClOrderId));
            message.Set(new OrderID(requestParams.OrderId));
            message.Set(new ClOrdID(requestParams.ClOrdId));
            /*
             * 
                new OrigClOrdID(requestParams.OrigClOrderId),
                new OrderID(requestParams.OrderId),
                new ClOrdID(requestParams.ClOrdId)
             */
            message.Header.GetString(Tags.BeginString);
            _tradeApp.SendMessage(message);

            await TradeLogsChannel.Writer.WriteAsync(new("OUT", "OrderCancelRequest", DateTimeOffset.UtcNow, $"Closing - OrderID:{requestParams.OrderId}# "));
        }
        private void SendSecurityListRequest()
        {
            QuickFix.FIX44.SecurityListRequest securityListRequest = new(new SecurityReqID("symbols"), new SecurityListRequestType(0));

            _tradeApp.SendMessage(securityListRequest);
        }
        public void SendLogoutRequest()
        {
            QuickFix.FIX44.Logout logoutRequest = new();
            _tradeApp.SendMessage(logoutRequest);
            _quoteApp.SendMessage(logoutRequest);
        }

        private void SendPositionsRequest()
        {
            positionsCount = -1;
            QuickFix.FIX44.RequestForPositions message = new();
            message.PosReqID = new PosReqID("Positions");
            if (_lp.Equals("CENTROID", StringComparison.OrdinalIgnoreCase))
            {
                message.Account = new Account(_account);
                message.AccountType = new AccountType(1);
                message.TransactTime = new TransactTime(DateTime.UtcNow);
            }
            _tradeApp.SendMessage(message);
        }

        private void SendMarketDataRequest(bool subscribe, int symbolId)
        {
            QuickFix.FIX44.MarketDataRequest message = new(new("MARKETDATAID"), new(subscribe ? '1' : '2'), new(1));

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup bidMarketDataEntryGroup = new() { MDEntryType = new MDEntryType('0') };
            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup offerMarketDataEntryGroup = new() { MDEntryType = new MDEntryType('1') };
            message.AddGroup(bidMarketDataEntryGroup);
            message.AddGroup(offerMarketDataEntryGroup);

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new() { Symbol = new QuickFix.Fields.Symbol(symbolId.ToString(CultureInfo.InvariantCulture)) };
            message.AddGroup(symbolGroup);

            _quoteApp.SendMessage(message);
        }
        private void SendMarketDataRequest(bool subscribe, string symbol)
        {
            QuickFix.FIX44.MarketDataRequest message = new(new("MARKETDATA_" + symbol), new(subscribe ? '1' : '2'), new(1));

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup bidMarketDataEntryGroup = new() { MDEntryType = new MDEntryType('0') };
            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup offerMarketDataEntryGroup = new() { MDEntryType = new MDEntryType('1') };
            message.AddGroup(bidMarketDataEntryGroup);
            message.AddGroup(offerMarketDataEntryGroup);

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new() { Symbol = new QuickFix.Fields.Symbol(symbol) };
            message.AddGroup(symbolGroup);

            _quoteApp.SendMessage(message);
        }

        private void SendOrderMassStatusRequest()
        {
            QuickFix.FIX44.OrderMassStatusRequest message = new(new MassStatusReqID("Orders"), new MassStatusReqType(7));

            _tradeApp.SendMessage(message);
        }
    }

    public record ApiCredentials(string QuoteHost, string TradeHost, int QuotePort, int TradePort, string QuoteSenderCompId, string TradeSenderCompId, string? QuoteSenderSubId, string? TradeSenderSubId, string QuoteTargetCompId, string TradeTargetCompId, string? QuoteTargetSubId, string? TradeTargetSubId, string QuoteUsername, string QuotePassword, string TradeUsername, string TradePassword, string TradeResetOnLogin, string QuoteResetOnLogin, string TradeSsl, string QuoteSsl, string? Account);

    public record Log(string Type, string MessageType, DateTimeOffset Time, string Message);

    public record ExecutionReport(string ExecId, char Type, Order Order, string ClOrderId);

    public record NewOrderRequestParameters(string Type, string ClOrdId, int? SymbolId, string? SymbolName, string TradeSide, decimal Quantity, decimal TargetPrice)
    {
        //public double TargetPrice { get; init; }

        public DateTime? Expiry { get; init; }

        public long? PositionId { get; init; }

        public string Designation { get; init; }
    }
    public record OrderAmmendRequest(string ClOrderId, string OrderType, string? OrderId, decimal OrderQty, decimal TargetPrice, DateTime? Expiry);
    public record OrderCancelRequestParameters(string OrigClOrderId, string OrderId, string ClOrdId);
}

