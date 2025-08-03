#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


using Newtonsoft.Json;

namespace TflNetworkBuilder
{
    public class StationNode
    {
        public string StationId { get => Station.StationId; set { } }

        public string Name => string.Join("/",Station.MatchedStop.Select(m => m.Name));
        public required Station Station { get; init; }
        [JsonIgnore]
        public List<StationNode> Next { get; set; } = [];
        [JsonIgnore]
        public List<StationNode> Prev { get; set; } = [];

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

    public class StationLink
    {
        public required StationNode From { get; init; }
        public required StationNode To { get; init; }

        public required string LineId { get; init; }

        [JsonIgnore]
        public double StartLat => From.Station.Lat;
        [JsonIgnore]
        public double StartLon => From.Station.Lon;
        [JsonIgnore]
        public double EndLat => To.Station.Lat;
        [JsonIgnore]
        public double EndLon => To.Station.Lon;

        public override bool Equals(object? obj)
        {
            if (obj is not StationLink right)
                return false;
            return From.Equals(right.From.StationId) && To.Equals(right.To.StationId) && LineId.Equals(right.LineId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From.StationId, To.StationId, LineId);
        }
    }

}