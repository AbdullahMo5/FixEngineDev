using FixEngine.Enitity;
using FixEngine.Services;
using Services;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Managers
{
    public class ExecutionManager
    {
        private ILogger<ExecutionManager> _logger;
        private ExecutionService _service;
        public event Action<string, Execution> executionEvent;

        public ExecutionManager(ILogger<ExecutionManager> logger, ExecutionService service)
        {
            _logger = logger;
            _service = service;
        }
        public async Task Process(ExecutionReport execution, string ctxtId)
        {
            //TODO:
            //fetch username by ctxdId => insert into db
            //invoke action
            var exec = new Execution()
                {
                    ExecId = execution.ExecId,
                    OrderId = execution.Order.Id.ToString(),
                    ClOrdId = execution.ClOrderId.ToString(),
                    PosId = execution.Order.PositionId.ToString(),
                    ExecType = execution.Type.ToString(),
                    OrdStatus = execution.Order.OrderStatus.ToString(),
                    SymbolId = execution.Order.SymbolId.ToString(),
                    Symbol = execution.Order.SymbolName.ToString(),
                    Side = execution.Order.TradeSide,
                    TransactTime = execution.Order.Time,
                    AvgPx = execution.Order.AvgPx,
                    OrderQty = execution.Order.Volume,
                    LeavesQty = execution.Order.LeavesQty,
                    CumQty = execution.Order.CumQty,
                    OrdType = execution.Order.Type,
                    Price = execution.Order.Price,
                    StopPrice = execution.Order.StopPrice,
                    TimeInForce = execution.Order.TimeInForce,
                    ExpireTime = execution.Order.ExpireTime,
                    Text = execution.Order.Text,
                    OrdRejReason = execution.Order.OrderRejReason,
                    Designation = execution.Order.Designation
                };
            bool res = await _service.Insert(exec);
            _logger.LogInformation($"Execution ID: {exec.ExecId} , OrderID: {exec.OrderId} db insert result: {res}");
            executionEvent?.Invoke(ctxtId, exec);
            
        }
    }
}
