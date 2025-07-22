using tfl_stats.Tfl;
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


namespace TflNetworkBuilder
{
    internal class Branch
    {
        private readonly StopPointSequence _sequence;

        public Branch(StopPointSequence sequence)
        {
            _sequence = sequence;
        }

        public int BranchId => (int)_sequence.BranchId!;
        public StopPointSequence StopPointSequence => _sequence;

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