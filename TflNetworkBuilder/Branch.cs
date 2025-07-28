using tfl_stats.Tfl;
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


namespace TflNetworkBuilder
{
    public class Branch
    {
        private StopPointSequence _sequence;

        public Branch(StopPointSequence sequence)
        {
            _sequence = sequence;
        }

        public int BranchId
        {
            get
            {
                return (int)_sequence.BranchId!;
            }
            set
            {
                // ignore
            }
        }
        public StopPointSequence StopPointSequence
        {
            get
            {
                return _sequence;
            }
            set
            {
                _sequence = value;
            }
        }

        public MatchedStop First => _sequence.StopPoint.First();
        public MatchedStop Last => _sequence.StopPoint.Last();

        public bool StartsAt(StationNode stationNode) => (First.ParentId ?? First.Id) == stationNode.StationId;
        public bool EndsAt(StationNode stationNode) => (Last.ParentId ?? Last.Id) == stationNode.StationId;

        public bool Visits(StationNode stationNode) => _sequence.StopPoint.Any(sp => (sp.ParentId ?? sp.Id) == stationNode.StationId);

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


}