using Managers;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
namespace FixEngine.Services
{
    public class ApiService
    {
        private ILogger<ApiService> _logger;
        private ExecutionManager _executionManager;
        private SymbolService _symbolService;
        private ConcurrentDictionary<string, FixClient> _clients = new();
        public ApiService(ILogger<ApiService> logger, ExecutionManager executionManager, SymbolService symbolService)
        {
            _logger = logger;
            _executionManager = executionManager;
            _symbolService = symbolService;

        }
        public void ConnectClient(ApiCredentials apiCredentials, string id, string lp)
        {
            var client = new FixClient(apiCredentials, lp, _symbolService);

            client.Connect();

            _clients.AddOrUpdate(id, client, (id, oldClient) => client);
            //ConsumeClient(id);
            //TODO: start consumer
        }

        public FixClient? GetClient(string id) {
            return _clients.ContainsKey(id) ? _clients[id] : null;
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
