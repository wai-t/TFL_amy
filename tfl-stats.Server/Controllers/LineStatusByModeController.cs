using Microsoft.AspNetCore.Mvc;
using tfl_stats.Server.Models;
using tfl_stats.Server.Services;

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
            var response = await _lineService.GetLine();

            if (!response.IsSuccessful)
            {
                switch (response.ResponseStatus)
                {

                    case ResponseStatus.NotFound:
                        _logger.LogWarning("Not Found");
                        return NotFound();

                    default:
                        _logger.LogError("Unexpected Error");
                        return StatusCode(500);
                }
            }
            _logger.LogInformation("Successfully fetched Lines.");
            return Ok(response.Data);
        }
    }
}
