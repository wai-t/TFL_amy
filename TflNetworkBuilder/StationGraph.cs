using System.Text.Json.Nodes;
using tfl_stats.Tfl;
using Xunit;
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


namespace TflNetworkBuilder
{

    public class StationGraph
    {
        private readonly HashSet<StationNode> _nodes = [];
        private readonly HashSet<Branch> _branches = [];

        public StationNode START_NODE { get; } = new() { Station = new() { StationId = "START" } };
        public StationNode END_NODE { get; } = new() { Station = new() { StationId = "END" } };

        public void AddBranch(StopPointSequence stopPointSequence)
        {
            Assert.True(_branches.Add(new Branch(stopPointSequence)));
        }

        public List<StationNode> Construct()
        {
            BuildStationNetwork();
            var ret = OrderStations();
            return ret;
        }

        private class BranchStatus
        {
            public bool Complete = false;
            public List<StationNode> BranchNodes = [];
        }
        private class PendingJoin
        {
            public required StationNode Id;
            public required Dictionary<StationNode, BranchStatus> JoiningBranches;
        }

        private List<StationNode> OrderStations()
        {
            var ret = ProcessFork(START_NODE);
            return ret;
        }

        Dictionary<string, PendingJoin> _pendingJoins = [];

        private List<StationNode> ProcessSequence(StationNode? pred, StationNode node)
        {
            if (node == END_NODE)
                return [];
            else if (node.IsMergePoint() && pred!=null)
            {
                var (completed, mergeResult) = ProcessMerge(node, pred, []);
                if (!completed)
                    return [];
                var ret2 = ProcessSequence(null, node);
                return [..mergeResult, ..ret2];
            }
            else if (node.IsForkPoint())
            {
                var splitResult = ProcessFork(node);
                return splitResult;
            }
            else
            {
                List<StationNode> ret = [];

                ret.Add(node);
                var currentNode = node.GetNext();

                while (currentNode.IsPassThru() && currentNode != END_NODE)
                {
                    ret.Add(currentNode);
                    currentNode = currentNode.GetNext();
                };

                var ret2 = ProcessSequence(ret.Last(), currentNode);
                return [.. ret, .. ret2];

            }

        }
        private (bool,List<StationNode>) ProcessMerge(StationNode node, StationNode pred, List<StationNode> stationsOnBranch)
        {
            if (!_pendingJoins.TryGetValue(node.Id, out var pendingJoin))
            {
                pendingJoin = new PendingJoin() {
                    Id = node,
                    JoiningBranches = node.Prev.ToDictionary(n => n, n => new BranchStatus() { Complete = false, BranchNodes = [] }) 
                };
                _pendingJoins.Add(node.Id, pendingJoin);
            }

            pendingJoin.JoiningBranches[pred] = new BranchStatus() {
                Complete = true,
                BranchNodes = stationsOnBranch 
            };

            if (pendingJoin.JoiningBranches.Values.All(t => t.Complete))
            {
                 var ret = (true, pendingJoin.JoiningBranches.Values
                    .OrderByDescending(t => t.BranchNodes.Count)
                    .SelectMany(t => t.BranchNodes).ToList());
                _pendingJoins.Remove(node.Id);

                return ret;
            }

            return (false, []);
        }

        private List<StationNode> ProcessFork(StationNode node)
        {
            List<List<StationNode>> threads = [];
            
            foreach(var head in node.Next)
            {
                var threadResult = ProcessSequence(node, head).ToList();
                threads.Add(threadResult);
            }

            threads = threads.OrderBy(t => t.Count).ToList();
            var ret = threads.SelectMany(t => t).ToList();
            ret.Insert(0, node);
            return ret;
        }
        private void BuildStationNetwork()
        {
            Stack<Branch> workQueue = new(_branches.Where(b => !b.StopPointSequence.PrevBranchIds.Any()
                                        || b.StopPointSequence.PrevBranchIds.Contains((int)b.StopPointSequence.BranchId!))
                                                                    );
            var remainingBranches = _branches.Except(workQueue);

            while (workQueue.Any())
            {
                StationNode? prevStation = null;
                var branch = workQueue.Pop();

                foreach (var stopPoint in branch.StopPointSequence.StopPoint)
                {
                    if (prevStation != null && stopPoint.Id == branch.StopPointSequence.StopPoint.First().Id)
                        break; // break the Circle Line
                    prevStation = Add(branch, stopPoint, prevStation);
                }

                foreach (var branchId in branch.StopPointSequence.NextBranchIds.Where(i => i != branch.StopPointSequence.BranchId))
                {
                    var nextBranch = remainingBranches.SingleOrDefault(b => b.BranchId == branchId);
                    if (nextBranch != null)
                        workQueue.Push(nextBranch);
                }
            }
            foreach (var station in _nodes)
            {
                if (station.IsTail())
                {
                    station.AddNext(END_NODE);
                }
            }

        }

        private StationNode Add(Branch branch, MatchedStop stopPoint, StationNode? prev)
        {
            var node = GetOrCreate(branch.BranchId, stopPoint);

            if (prev != null && !node.Follows(prev))
            {
                node.RemovePrev(START_NODE);
                node.AddPrev(prev);
            }
            else if (prev == null && node.IsHead()
                && !branch.StopPointSequence.PrevBranchIds.Where(id => id != branch.StopPointSequence.BranchId).Any())
            {
                node.AddPrev(START_NODE);
            }
            else
            {
                Assert.True(true);
            }

            return node;

        }

        private StationNode GetOrCreate(int branchId, MatchedStop stopPoint)
        {
            var stationId = stopPoint.ParentId ?? stopPoint.Id;

            var entry = _nodes.SingleOrDefault(n => n.Id == stationId);
            if (entry is null)
            {
                entry = new StationNode()
                {
                    Station = new Station()
                    {
                        StationId = stationId,
                        BranchIds = [branchId],
                        MatchedStop = [stopPoint]
                    }
                };
                _nodes.Add(entry);
            }
            else
            {
                Assert.Equal(stationId, entry.Station.StationId);

                var station = entry.Station;
                if (!station.BranchIds.Contains(branchId))
                    station.BranchIds.Add(branchId);
                if (!station.MatchedStop.Any(s => s.Id == stopPoint.Id))
                    station.MatchedStop.Add(stopPoint);
            }
            return entry;
        }

        //
        // Debugging and Testing Helpers
        //

        public record PrintableNode(string Id, List<string> Names, List<String> Prev, List<string> Next, List<int> BranchIds);

        public List<PrintableNode> DumpNodes()
        {
            List<PrintableNode> output = [];
            foreach (var node in _nodes)
            {
                output.Add(new PrintableNode(
                    node.Id,
                    [.. node.Station.MatchedStop.Select(s => $"{s.Id} {s.ParentId} {s.Name}")],
                    [.. node.Prev.Select(n => n.Id)],
                    [.. node.Next.Select(n => n.Id)],
                    node.Station.BranchIds
                ));
            }
            return output;
        }

    }


}