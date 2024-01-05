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

namespace FixEngine.Hubs
{
    public class TradeHub : Hub
    {
        private readonly ApiService _apiService;
        private readonly ILogger<TradeHub> _logger;
        private ExecutionManager _executionManager;
        private ExecutionService _executionService;
        public TradeHub(ApiService apiService, ILogger<TradeHub> logger, ExecutionManager executionManager, ExecutionService executionService)
        {
            _apiService = apiService;
            _logger = logger;
            _executionManager = executionManager;
            _executionService = executionService;
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
        public async Task ConnectCtrader()
        {
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
            _logger.LogInformation("Connecting... |"+ Context.ConnectionId.ToString());
            _apiService.ConnectClient(apiCredentials, Context.ConnectionId);
            _logger.LogInformation("Connected succcessfully");

            await Clients.Caller.SendAsync("Connected");
            //await PositionsIndexed(new CancellationToken());
        }
        
        public async Task Disconnect(string username)
        {
            _logger.LogInformation("Disconnecting... |" + Context.ConnectionId.ToString());
            var client = _apiService.GetClient(Context.ConnectionId);
            if(client != null)
            {
                _logger.LogInformation($"{Context.ConnectionId.ToString()}- Initiating logout.. ");
                client.SendLogoutRequest();
                _logger.LogInformation($"{Context.ConnectionId.ToString()}- Initiating client dispose.. ");
                client.Dispose();
                _logger.LogInformation($"{Context.ConnectionId.ToString()}- Client Disposed");
                _logger.LogInformation("Disconnected succcessfully");
                return ;
            }
            _logger.LogInformation($"Client: {Context.ConnectionId.ToString()} not found");

        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Disconnecting... |" + Context.ConnectionId.ToString());
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
                _logger.LogInformation($"Client: {Context.ConnectionId.ToString()} not found");
            return base.OnDisconnectedAsync(exception);
        }

        public void SendNewOrderRequest(NewOrderRequestParameters parameters)
        {
            _logger.LogInformation("New order request ");
            var client = _apiService.GetClient(Context.ConnectionId);
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
        public void SendCloseOrderRequest(NewOrderRequestParameters parameters)
        {
            _logger.LogInformation("Close order request ");
            var client = _apiService.GetClient(Context.ConnectionId);
            if(client != null)
            {
                //fetch from db
                string ?posId = parameters.PositionId.ToString();
                if (!string.IsNullOrEmpty(posId))
                {
                    Enitity.Execution ? execution =_executionService.FetchByPositionId(posId);
                    if (execution != null)
                    {
                        DateTime currentTime = DateTime.UtcNow;
                        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                        _logger.LogInformation("Sending order cancel request");
                        client.SendOrderCancelRequest(new OrderCancelRequestParameters(OrigClOrderId: execution.ClOrdId, OrderId: posId, ClOrdId: "" + unixTime));
                    }

                }
            }
            else
            {
                _logger.LogInformation("Client not found."); 
            }
        }

        public async IAsyncEnumerable<Services.Log> Logs([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(Context.ConnectionId);
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

        public async IAsyncEnumerable<Common.Symbol> Symbols([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(Context.ConnectionId);
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

        public async IAsyncEnumerable<SymbolQuote> SymbolQuotes([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(Context.ConnectionId);
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
        public async IAsyncEnumerable<Position> Positions([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(Context.ConnectionId);
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
        public async IAsyncEnumerable<ExecutionReport> ExecutionReport([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _apiService.GetClient(Context.ConnectionId);
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
    }
}
