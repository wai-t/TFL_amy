using tfl_stats.Tfl;
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


namespace TflNetworkBuilder
{
    public class Station
    {
        public required string StationId { get; set; }
        public int Index { get; set; } = int.MinValue;
        public List<int> BranchIds { get; set; } = [];

        public List<MatchedStop> MatchedStop { get; set; } = [];

        public override bool Equals(object? obj)
        {
            return obj is Station right && StationId == right.StationId;
        }

        public override int GetHashCode()
        {
            return StationId.GetHashCode();
        }
    }


}