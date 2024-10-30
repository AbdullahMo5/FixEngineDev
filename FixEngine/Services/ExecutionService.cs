using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Services;

namespace Services
{
    public class ExecutionService : GenericService<Execution>, IExecutionService
    {
        public ExecutionService(DatabaseContext context) : base(context) { }
    }
}
