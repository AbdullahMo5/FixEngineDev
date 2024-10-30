using FixEngine.Entity;
using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/secure/[controller]")]
    [ApiController]
    public class ExecutionController : ControllerBase
    {
        private readonly ILogger<ExecutionController> _logger;
        private readonly IExecutionService _executionService;

        public ExecutionController(ILogger<ExecutionController> logger, IExecutionService executionService)
        {
            _logger = logger;
            _executionService = executionService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
          => Ok(await _executionService.GetAllAsync());

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var execution = await _executionService.GetByIdAsync(id);
            if (execution is null)
                return NotFound("There is no execution with this id");
            return Ok(execution);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Execution model)
        {
            if (await _executionService.AddAsync(model) > 0)
                return Ok("Execution Added Success");
            return BadRequest("Something went wring");
        }

        [HttpPut]
        public async Task<IActionResult> Update(int executionId, Execution execution)
        {
            if (execution.Id != executionId)
                return BadRequest("Id in paremeters does not match id in the body");

            var isExecutionExist = await _executionService.IsExist(e => e.Id == execution.Id);
            if (isExecutionExist)
                return BadRequest("There is no Execution with this id");

            var executionFromDb = await _executionService.GetByIdAsync(execution.Id);
            if (executionFromDb is null)
                return BadRequest("There is no gateway with this id");


            executionFromDb.Designation = execution.Designation;
            executionFromDb.ClOrdId = execution.ClOrdId;
            executionFromDb.ExecId = execution.ExecId;
            executionFromDb.OrderId = execution.OrderId;
            executionFromDb.PosId = execution.PosId;
            executionFromDb.AvgPx = execution.AvgPx;
            executionFromDb.CumQty = execution.CumQty;
            executionFromDb.TransactTime = execution.TransactTime;
            executionFromDb.ExecType = execution.ExecType;
            executionFromDb.ExpireTime = execution.ExpireTime;
            executionFromDb.LeavesQty = execution.LeavesQty;
            executionFromDb.OrderQty = execution.OrderQty;
            executionFromDb.OrdRejReason = execution.OrdRejReason;
            executionFromDb.OrdStatus = execution.OrdStatus;
            executionFromDb.OrdType = execution.OrdType;
            executionFromDb.Price = execution.Price;
            executionFromDb.StopPrice = execution.StopPrice;
            executionFromDb.Side = execution.Side;
            executionFromDb.Symbol = execution.Symbol;
            executionFromDb.SymbolId = execution.SymbolId;
            executionFromDb.Text = execution.Text;
            executionFromDb.TimeInForce = execution.TimeInForce;
            executionFromDb.TransactTime = execution.TransactTime;
            if (await _executionService.Update(executionFromDb) > 0)
                return Ok("Updated success");

            return BadRequest("Something went wrong");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var execution = await _executionService.GetByIdAsync(id);
            if (execution is null)
                return BadRequest("There is no execution with this id");
            if (await _executionService.Delete(execution) > 0)
                return Ok("Deleted successfully");
            return BadRequest("Something went wrong");
        }
    }
}
