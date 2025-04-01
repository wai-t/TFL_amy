using Microsoft.AspNetCore.Mvc;
using tfl_stats.Server.Services.JourneyService;

namespace tfl_stats.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StopPointController : ControllerBase
    {
        private JourneyService _journeyService;
        private ILogger<StopPointController> _logger;

        public StopPointController(JourneyService journeyService, ILogger<StopPointController> logger)
        {
            _journeyService = journeyService;
            _logger = logger;
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetAutocompleteSuggestions([FromQuery] string query)
        {
            var data = await _journeyService.GetAutocompleteSuggestions(query);
            _logger.LogInformation(data.Count == 0 ? "No data fetched" : "Data fetched");

            return Ok(data);
        }
    }
}
