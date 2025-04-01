using System.ComponentModel.DataAnnotations;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class JourneyRequest
    {
        [Required]
        public string From { get; set; } = string.Empty;

        [Required]
        public string To { get; set; } = string.Empty;
    }
}
