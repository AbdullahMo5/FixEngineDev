using FixEngine.Data;
using FixEngine.Enitity;

namespace Services
{
    public class ExecutionService
    {
        private ILogger<ExecutionService> _logger;
        private DatabaseContext _databaseContext;
        public ExecutionService(ILogger<ExecutionService> logger, DatabaseContext context)
        {
            _logger = logger;
            _databaseContext = context;
        }
        public string TestFetch()
        {
            return "FETCHED EXECUTIONS";
        }
        public List<Execution> FetchAll()
        {
            return _databaseContext.executions.ToList();
        }
        public async Task<bool> Insert(Execution execution)
        {
            try
            {
                _logger.LogInformation("Inserting execution");
                this._databaseContext
                    .Add(execution);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation("Success...");
                return true;

            }catch (Exception ex)
            {
                _logger.LogInformation($"Failed to insert {ex.Message}");   
                return false;
            }
        }
        public Execution ? FetchByPositionId(string positionId)
        {
            return _databaseContext.executions.FirstOrDefault(position => position.PosId == positionId);
        }
    }
}
