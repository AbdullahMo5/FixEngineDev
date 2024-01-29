using FixEngine.Services;
using Common;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FixEngine.Controllers;
using Managers;
using Services;
using QuickFix.Fields;
using FixEngine.Shared;

namespace FixEngine.Hubs
{
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

        public async Task Ping() {

            _logger.LogInformation("Pong... |", Context.ConnectionId);
            await Clients.Caller.SendAsync("Pong", "Pong");
        }
        public async Task IsConnected()
        {
            var client = _apiService.GetClient(Context.ConnectionId);
            string status = (client != null) ? "Connected" : "Disconnected";
            await Clients.Caller.SendAsync("ConnectionStatus", status);
        }
        public async Task Connect(ApiCredentials apiCredentials)
        {
            _logger.LogInformation("Connecting... |", Context.ConnectionId.ToString());
            _apiService.ConnectClient(apiCredentials, Context.ConnectionId);
            _logger.LogInformation("Connected succcessfully");
            await Clients.Caller.SendAsync("Connected");
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
            if (string.IsNullOrEmpty(token)) {
                _logger.LogError("Connecting failed. Reason: Token is null or empty");
                return;
            }
            bool sessionValid = _sessionManager.IsExist(token);
            if(!sessionValid)
            {
                _logger.LogError("Client FIX Connection failed. Reason: Invalid session token");
                return;
            }
            var client = _apiService.GetClient(token);
            if(client == null)
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
                    Username: "4024137",
                    Password: "Gtlfx125");
                _logger.LogInformation("Connecting... |" + token);// Context.ConnectionId.ToString());
                _apiService.ConnectClient(apiCredentials, token);//Context.ConnectionId);
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
            _logger.LogInformation("Disconnecting... |" + token) ;
            var client = _apiService.GetClient(token);//Context.ConnectionId);
            if(client != null)
            {
                _logger.LogInformation($"{/*Context.ConnectionId.ToString()*/token}- Initiating logout.. ");
                client.SendLogoutRequest();
                _logger.LogInformation($"{/*Context.ConnectionId.ToString()*/token}- Initiating client dispose.. ");
                client.Dispose();
                _logger.LogInformation($"{/*Context.ConnectionId.ToString()*/token}- Client Disposed");
                _logger.LogInformation("Disconnected succcessfully");
                return ;
            }
            _logger.LogInformation($"Client: {/*Context.ConnectionId.ToString()*/ token} not found");

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
            return base.OnDisconnectedAsync(exception);
        }

        public void SendOrderAmmendRequest(string token, OrderAmmendRequest parameters)
        {
            _logger.LogInformation("Order ammend request ");
            var client = _apiService.GetClient(token);//Context.ConnectionId);
            if(client != null)
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
            if(client != null)
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
            if(client != null)
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

        public async IAsyncEnumerable<Services.Log> Logs(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(token);
            if(client != null)
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

        public async IAsyncEnumerable<Common.Symbol> Symbols(string token,[EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if(client != null)
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

        public async IAsyncEnumerable<SymbolQuote> SymbolQuotes(string token,[EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quotes Token => ", token);
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if(client != null)
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
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if(client != null )
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
        public async IAsyncEnumerable<ExecutionReport> ExecutionReport(string token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if(client != null)
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
            var client = _apiService.GetClient(/*Context.ConnectionId*/token);
            if(client != null)
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
    }
}
