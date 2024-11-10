using Common;
using FixEngine.Services;
using FixEngine.Shared;
using Managers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace FixEngine.Hubs
{
    [Authorize]
    public class TradeHub : Hub
    {
        private readonly ApiService _apiService;
        private readonly ILogger<TradeHub> _logger;
        private ExecutionManager _executionManager;
        private ExecutionService _executionService;
        private SessionManager _sessionManager;
        public TradeHub(ApiService apiService, ILogger<TradeHub> logger, ExecutionManager executionManager, ExecutionService executionService, SessionManager sessionManager)
        {
            _apiService = apiService;
            _logger = logger;
            _executionManager = executionManager;
            _executionService = executionService;
            _sessionManager = sessionManager;
        }
        public async Task Ping()
        {
            _logger.LogInformation("Pong... |", Context.ConnectionId);
            await Clients.Caller.SendAsync("Pong", "Pong");
        }
        public async Task IsConnected()
        {
            var client = _apiService.GetClient(Context.ConnectionId);
            string status = (client != null) ? "Connected" : "Disconnected";
            await Clients.Caller.SendAsync("ConnectionStatus", status);
        }
        public async Task Connect(ApiCredentials apiCredentials, string lp)
        {
            _logger.LogInformation("Connecting... |", Context.ConnectionId.ToString());
            _apiService.ConnectClient(apiCredentials, Context.ConnectionId, lp);
            _logger.LogInformation("Connected succcessfully");
            await Clients.Caller.SendAsync("Connected");
        }
        public async Task ConnectCentroid(string token)
        {

            _logger.LogInformation("Connecting centroid client with token => ", token);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Connecting failed. Reason: Token is null or empty");
                await Clients.Caller.SendAsync("Disconnected");
            }
            bool sessionValid = _sessionManager.IsExist(token);
            if (!sessionValid)
            {
                _logger.LogError("Client FIX Connection failed. Reason: Invalid session token");
                await Clients.Caller.SendAsync("Disconnected");
                return;
            }
            var client = _apiService.GetClient(token);
            if (client == null)
            {
                //check if session exists
                ApiCredentials apiCredentials = new ApiCredentials(
                QuoteHost: "crfuk.centroidsol.com",
                TradeHost: "crfuk.centroidsol.com",
                QuotePort: 53810,
                TradePort: 53811,
                QuoteSenderCompId: "MD_Fintic-FIX-TEST",
                TradeSenderCompId: "TD_Fintic-FIX-TEST",
                null,
                null,
                //QuoteSenderSubId: "testcentroid",// + token,
                //TradeSenderSubId: "testcentroid",// + token,
                QuoteTargetCompId: "CENTROID_SOL",
                TradeTargetCompId: "CENTROID_SOL",
                null,
                null,
                //QuoteTargetSubId: "QUOTE",
                //TradeTargetSubId: "TRADE",
                QuoteUsername: "Fintic-FIX-TEST",
                QuotePassword: "#oB*sFb6",
                TradeUsername: "Fintic-FIX-TEST",
                TradePassword: "#oB*sFb6", //"123Nm,.com",
                TradeResetOnLogin: "N",
                TradeSsl: "Y",
                QuoteResetOnLogin: "Y",
                QuoteSsl: "N",
                Account: "Fintic-Fix-Test"
                );
                _logger.LogInformation("Connecting... |" + token);
                _apiService.ConnectClient(apiCredentials, token, "CENTROID");
                _logger.LogInformation("Connected succcessfully");

                await Clients.Caller.SendAsync("Connected");

            }
            else
            {
                await Clients.Caller.SendAsync("Connected");
            }

        }
        public async Task ConnectCtrader(string token)
        {
            //check if session with token exists
            /*//acc1
            ApiCredentials apiCredentials = new ApiCredentials(
                QuoteHost: "h74.p.ctrader.com",
                TradeHost: "h74.p.ctrader.com",
                QuotePort: 5201,
                TradePort: 5202,
                QuoteSenderCompId: "demo.ctrader.3873996",
                TradeSenderCompId: "demo.ctrader.3873996",
                QuoteSenderSubId: "3873996"+ Context.ConnectionId.ToString(),
                TradeSenderSubId: "3873996"+ Context.ConnectionId.ToString(),
                QuoteTargetCompId: "cServer",
                TradeTargetCompId: "cServer",
                QuoteTargetSubId: "QUOTE",
                TradeTargetSubId: "TRADE",
                Username: "3873996",
                Password: "Gtlfx125");
            */
            //acc2
            _logger.LogInformation("Connecting with token => ", token);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Connecting failed. Reason: Token is null or empty");
                await Clients.Caller.SendAsync("Disconnected");
            }
            bool sessionValid = _sessionManager.IsExist(token);
            if (!sessionValid)
            {
                _logger.LogError("Client FIX Connection failed. Reason: Invalid session token");
                await Clients.Caller.SendAsync("Disconnected");
                return;
            }
            var client = _apiService.GetClient(token);
            if (client == null)
            {
                //check if session exists
                ApiCredentials apiCredentials = new ApiCredentials(
                    QuoteHost: "h74.p.ctrader.com",
                    TradeHost: "h74.p.ctrader.com",
                    QuotePort: 5201,
                    TradePort: 5202,
                    QuoteSenderCompId: "demo.ctrader.4024137",
                    TradeSenderCompId: "demo.ctrader.4024137",
                    QuoteSenderSubId: "4024137" + token,//Context.ConnectionId.ToString(),
                    TradeSenderSubId: "4024137" + token,//Context.ConnectionId.ToString(),
                    QuoteTargetCompId: "cServer",
                    TradeTargetCompId: "cServer",
                    QuoteTargetSubId: "QUOTE",
                    TradeTargetSubId: "TRADE",
                    QuoteUsername: "4024137",
                    QuotePassword: "Gtlfx125",
                    TradeUsername: "4024137",
                    TradePassword: "Gtlfx125",
                    TradeResetOnLogin: "N",
                    TradeSsl: "N",
                    QuoteResetOnLogin: "N",
                    QuoteSsl: "N",
                    Account: null
                    );
                _logger.LogInformation("Connecting... |" + token);// Context.ConnectionId.ToString());
                _apiService.ConnectClient(apiCredentials, token, "CTRADER");//Context.ConnectionId);
                _logger.LogInformation("Connected succcessfully");

                await Clients.Caller.SendAsync("Connected");

            }
            else
            {
                await Clients.Caller.SendAsync("Connected");
            }

            //await PositionsIndexed(new CancellationToken());
        }

        public async Task Disconnect(string token)
        {
            _logger.LogInformation("Disconnecting... |" + token);
            var client = _apiService.GetClient(token);//Context.ConnectionId);
            if (client != null)
            {
                _logger.LogInformation($"{token}- Initiating logout.. ");
                client.SendLogoutRequest();
                _logger.LogInformation($"{token}- Initiating client dispose.. ");
                client.Dispose();
                _logger.LogInformation($"{token}- Client Disposed");
                _logger.LogInformation("Disconnected succcessfully");
                await Clients.Caller.SendAsync("Disconnected");
                return;
            }
            _logger.LogInformation($"Client: {token} not found");
            await Clients.Caller.SendAsync("Disconnected");

        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            /*_logger.LogInformation("Disconnecting... |" + Context.ConnectionId.ToString());
            _logger.LogInformation("Reason |", exception);

            var client = _apiService.GetClient(Context.ConnectionId);
            if (client != null)
            {
                _logger.LogInformation($"{Context.ConnectionId.ToString()}- Initiating logout.. ");
                client.SendLogoutRequest();
                _logger.LogInformation($"{Context.ConnectionId.ToString()}- Initiating client dispose.. ");
                client.Dispose();
                _logger.LogInformation($"{Context.ConnectionId.ToString()}- Client Disposed");
                _logger.LogInformation("Disconnected succcessfully");
            }
            else
                _logger.LogInformation($"Client: {Context.ConnectionId.ToString()} not found");*/
            Clients.Caller.SendAsync("Disconnected");
            return base.OnDisconnectedAsync(exception);
        }

        public void SendOrderAmmendRequest(string token, OrderAmmendRequest parameters)
        {
            _logger.LogInformation("Order ammend request ");
            var client = _apiService.GetClient(token);//Context.ConnectionId);
            if (client != null)
            {
                _logger.LogInformation("Sending order ammend request");
                client.SendOrderAmmendRequest(parameters);
            }
            else
            {
                _logger.LogInformation("Client not found.");
            }
        }
        public void SendNewOrderRequest(string token, NewOrderRequestParameters parameters)
        {
            _logger.LogInformation("New order request ");
            var client = _apiService.GetClient(token);//Context.ConnectionId);
            if (client != null)
            {
                _logger.LogInformation("Sending New order request");
                client.SendNewOrderRequest(parameters);
            }
            else
            {
                _logger.LogInformation("Client not found.");
            }
        }
        public void SendCloseOrderRequest(string token, string ClOrderId, string OrderId)
        {
            _logger.LogInformation("Close order request ");
            var client = _apiService.GetClient(token);
            if (client != null)
            {
                //fetch from db
                DateTime currentTime = DateTime.UtcNow;
                long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                _logger.LogInformation("Sending order cancel request");
                client.SendOrderCancelRequest(new OrderCancelRequestParameters(OrigClOrderId: ClOrderId, OrderId: OrderId, ClOrdId: "" + unixTime));


            }
            else
            {
                _logger.LogInformation("Client not found.");
            }
        }

        public async IAsyncEnumerable<Services.Log> TradeLogs(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(token);
            if (client != null)
            {
                while (await client.TradeLogsChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.TradeLogsChannel.Reader.TryRead(out var log))
                    {
                        yield return log;
                    }
                }

            }
        }
        public async IAsyncEnumerable<Services.Log> Logs(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(token);
            if (client != null)
            {
                while (await client.LogsChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.LogsChannel.Reader.TryRead(out var log))
                    {
                        yield return log;
                    }
                }

            }
        }

        public async IAsyncEnumerable<Common.Symbol> Symbols(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if (client != null)
            {
                while (await client.SecurityChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.SecurityChannel.Reader.TryRead(out var symbol))
                    {
                        yield return symbol;
                    }
                }

            }
        }

        public async IAsyncEnumerable<SymbolQuote> SymbolQuotes(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quotes Token => ", token);
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if (client != null)
            {
                while (await client.MarketDataSnapshotFullRefreshChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.MarketDataSnapshotFullRefreshChannel.Reader.TryRead(out var symbolQuote))
                    {
                        yield return symbolQuote;
                    }
                }

            }
        }
        public async IAsyncEnumerable<Position> Positions(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation("Positions Token => ", token);
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if (client != null)
            {
                while (await client.PositionReportChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.PositionReportChannel.Reader.TryRead(out var position))
                    {
                        yield return position;
                    }
                }

            }
        }

        public async Task StreamSymbolQuotes(string token, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Streaming Symbol Quotes. Token => ", token);

            var client = _apiService.GetClient(token);
            if (client != null)
            {
                while (await client.MarketDataSnapshotFullRefreshChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.MarketDataSnapshotFullRefreshChannel.Reader.TryRead(out var symbolQuote))
                    {
                        // Stream each symbol quote to the caller
                        await Clients.Caller.SendAsync("ReceiveSymbolQuote", symbolQuote, cancellationToken);
                    }
                }
            }
            else
            {
                _logger.LogWarning($"No client found for token {token}. Unable to stream symbol quotes.");
                await Clients.Caller.SendAsync("ReceiveSymbolQuoteError", "Client not found.");
            }
        }
        public async IAsyncEnumerable<ExecutionReport> ExecutionReport(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if (client != null)
            {
                while (await client.ExecutionReportChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.ExecutionReportChannel.Reader.TryRead(out var executionReport))
                    {
                        await _executionManager.Process(executionReport, Context.ConnectionId);
                        yield return executionReport;
                    }
                }

            }
        }
        public async IAsyncEnumerable<ExecutionReport> Orders(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation("Orders Token => ", token);
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if (client != null)
            {
                while (await client.OrdersExecutionReportChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.OrdersExecutionReportChannel.Reader.TryRead(out var executionReport))
                    {
                        await _executionManager.Process(executionReport, Context.ConnectionId);
                        yield return executionReport;
                    }
                }

            }
        }
        #region Test
        public void SendBOrderRequest(string token, NewOrderRequestParameters parameters)
        {
            var client = _apiService.GetClient(token);
            client.simulator.ReceiveOrder(parameters);
        }
        public async IAsyncEnumerable<Entity.Position> StreamBPositions(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(token);
            if (client != null)
            {
                while (await client.simulator.positionChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (client.simulator.positionChannel.Reader.TryRead(out var position))
                    {
                        yield return position;
                    }
                }
            }
        }
        #endregion
    }
}
