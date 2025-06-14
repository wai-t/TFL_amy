using Microsoft.AspNetCore.Mvc;
using tfl_stats.Server.Services;
namespace tfl_stats.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StopPointController : ControllerBase
    {
        private StopPointService _stopPointService;
        private ILogger<StopPointController> _logger;

        public StopPointController(StopPointService stopPointService, ILogger<StopPointController> logger)
        {
            _stopPointService = stopPointService;
            _logger = logger;
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetAutocompleteSuggestions([FromQuery] string query)
        {
            var data = await _stopPointService.GetAutocompleteSuggestions(query);
            _logger.LogInformation(data.Count == 0 ? "No data fetched" : "Data fetched");

            return Ok(data);
        }

        [HttpGet("stopPointList")]
        public async Task<IActionResult> GetStopPointList()
        {
            var data = await _stopPointService.GetStopPointList();
            _logger.LogInformation(data.Count == 0 ? "No data fetched" : "Data fetched");
            return Ok(data);
        }
    }
}