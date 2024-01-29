using Microsoft.AspNetCore.Mvc;
using Services;

namespace FixEngine.Controllers
{
    [Route("api/secure/[controller]")]
    [ApiController]
    public class ExecutionController : ControllerBase
    {
        private readonly ILogger<ExecutionController> _logger;
        private ExecutionService _executionService;
        public ExecutionController(ILogger<ExecutionController> logger, ExecutionService executionService)
        {
            _logger = logger;
            _executionService = executionService;
        }
        [HttpGet]
        public string Get()
        {
            return _executionService.TestFetch();
        }
        [HttpPost]
        public IActionResult Post()
        {
            return Ok();
        }
    }
}
