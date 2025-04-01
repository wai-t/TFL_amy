using Microsoft.AspNetCore.Mvc;
using tfl_stats.Server.Services.LineService;

namespace tfl_stats.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineStatusByModeController : ControllerBase
    {
        private LineService _lineService;
        private ILogger<LineStatusByModeController> _logger;

        public LineStatusByModeController(LineService lineService, ILogger<LineStatusByModeController> logger)
        {
            _lineService = lineService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetLine()
        {
            var data = await _lineService.getLine();
            _logger.LogInformation(data.Count == 0 ? "No data fetched" : "Data fetched");

            return Ok(data);
        }
    }
}
