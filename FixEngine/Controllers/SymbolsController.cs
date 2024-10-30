using FixEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SymbolsController : ControllerBase
    {
        private readonly ISymbolService _symbolService;

        public SymbolsController(ISymbolService symbolService)
        {
            _symbolService = symbolService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => Ok(await _symbolService.GetAllAsync());
    }
}
