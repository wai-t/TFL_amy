using Microsoft.AspNetCore.Mvc;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.JourneyModels;
using tfl_stats.Server.Services.JourneyService;

namespace tfl_stats.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly JourneyService _journeyService;
        private readonly ILogger<JourneyController> _logger;

        public JourneyController(JourneyService journeyService, ILogger<JourneyController> logger)
        {
            _journeyService = journeyService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GetJourney([FromBody] JourneyRequest journeyRequest)
        {
            var response = await _journeyService.GetJourney(journeyRequest);
            if (!response.IsSuccessful)
            {
                switch (response.ResponseStatus)
                {
                    case ResponseStatus.BadRequest:
                        _logger.LogError("Bad Request");
                        return BadRequest();

                    case ResponseStatus.NotFound:
                        _logger.LogWarning("Not Found");
                        return NotFound();

                    default:
                        _logger.LogError("Unexpected Error");
                        return StatusCode(500);
                }
            }

            _logger.LogInformation("Successfully fetched journeys.");
            return Ok(response.Data);
        }
    }
}
