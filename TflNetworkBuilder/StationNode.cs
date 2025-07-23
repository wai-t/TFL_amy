#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


namespace TflNetworkBuilder
{
    public class StationNode
    {
        public string StationId => Station.StationId;
        public required Station Station { get; init; }
        public List<StationNode> Next { get; init; } = [];
        public List<StationNode> Prev { get; init; } = [];

        public void AddNext(StationNode nextNode)
        {
            if (!Next.Contains(nextNode))
                Next.Add(nextNode);
            if (!nextNode.Prev.Contains(this))
                nextNode.Prev.Add(this);
        }

        public void AddPrev(StationNode prevNode)
        {
            if (!Prev.Contains(prevNode))
                Prev.Add(prevNode);
            if (!prevNode.Next.Contains(this))
                prevNode.Next.Add(this);
        }
        public void RemoveNext(StationNode nextNode)
        {
            if (Next.Contains(nextNode))
            {
                Next.Remove(nextNode);
                nextNode.Prev.Remove(this); 
            }
        }

        public void RemovePrev(StationNode prevNode)
        {
            prevNode.RemoveNext(this);
        }


        public bool Follows(StationNode node)
        {
            return Prev.Contains(node);
        }

        public bool Precedes(StationNode node)
        {
            return Next.Contains(node);
        }
        public bool IsMergePoint()
        {
            return Prev.Count > 1;
        }

        public bool IsForkPoint()
        {
            return Next.Count > 1;
        }

        public bool IsPassThru()
        {
            return !IsMergePoint() && !IsForkPoint();
        }

        public StationNode GetNext()
        {
            // this will throw if this is a fork (>1 next)
            return Next.Single();
        }

        public StationNode GetPrev()
        {
            // this will throw if this is a merge (>1 prev)
            return Prev.Single();
        }

        public bool IsHead()
        {
            return !Prev.Any();
        }

        public bool IsTail()
        {
            return !Next.Any();
        }
        public override bool Equals(object? obj)
        {
            return obj is StationNode right && StationId == right.StationId;
        }

        public override int GetHashCode()
        {
            return StationId.GetHashCode();
        }
    }


}