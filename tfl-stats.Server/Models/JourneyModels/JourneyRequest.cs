using System.ComponentModel.DataAnnotations;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class JourneyRequest
    {
        [Required]
        public string FromNaptanId { get; set; } = string.Empty;

        [Required]
        public string ToNaptanId { get; set; } = string.Empty;
    }
}
