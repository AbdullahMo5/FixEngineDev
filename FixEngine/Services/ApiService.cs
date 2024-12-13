using Managers;
using System.Collections.Concurrent;
namespace FixEngine.Services
{
    public class ApiService
    {
        private ILogger<ApiService> _logger;
        private ExecutionManager _executionManager;
        private SymbolService _symbolService;
        private ConcurrentDictionary<string, FixClient> _clients = new();
        private ConcurrentDictionary<int, FixClient> clients = new();           //For Test purpose only
        private OrderService _orderService;
        private PositionService _positionsService;
        private RiskUserService _riskUserService;
        private GroupService _groupService;
        public ApiService(ILogger<ApiService> logger, ExecutionManager executionManager,
            SymbolService symbolService, OrderService orderService, PositionService positionsService, RiskUserService riskUserService, GroupService groupService)
        {
            _logger = logger;
            _executionManager = executionManager;
            _symbolService = symbolService;
            _orderService = orderService;
            _positionsService = positionsService;
            Console.WriteLine("hello world!");
            _riskUserService = riskUserService;
            _groupService = groupService;
        }
        public async Task ConnectClient(ApiCredentials apiCredentials, string id, string lp)
        {
            var client = new FixClient(apiCredentials, lp, _symbolService, _orderService, _positionsService, _riskUserService, _groupService);

            client.Connect();

            _clients.AddOrUpdate(id, client, (id, oldClient) => client);
            clients.AddOrUpdate(1, client, (id, oldClient) => client);          //For Test purpose only
            //ConsumeClient(id);
            //TODO: start consumer
        }

        public FixClient? GetClient(string id)
        {
            return _clients.ContainsKey(id) ? _clients[id] : null;
        }

        public FixClient? GetClient(int id)                                  //For Test purpose only
        {
            return clients.ContainsKey(id) ? clients[id] : null;
        }

        public void RemoveClient(string id)
        {
            if (_clients.ContainsKey(id))
            {
                _clients[id].Dispose();
                _clients.TryRemove(id, out FixClient data);
            }
        }
        public void ConsumeClient(string id)
        {
            var client = GetClient(id);
            if (client != null)
            {
                Task taskExecutionReport = Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        while (await client.ExecutionReportChannel.Reader.WaitToReadAsync(new CancellationToken()))
                        {
                            while (client.ExecutionReportChannel.Reader.TryRead(out var executionReport))
                            {
                                await _executionManager.Process(executionReport, id);
                            }
                        }

                    }
                });

            }
            //fetch client using id
            //listen for channel data
            //process execution in execution manager
        }
    }
}
