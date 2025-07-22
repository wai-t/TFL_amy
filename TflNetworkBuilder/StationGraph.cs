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

        public StationNode StartNode { get; } = new() { Station = new() { StationId = "START" } };
        public StationNode EndNode { get; } = new() { Station = new() { StationId = "END" } };

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

        public class ThreadStatus
        {
            public bool Flagged = false;
            public List<StationNode> indexedStationNodes = [];
        }
        public class PendingJoin
        {
            public required StationNode Id;
            public required Dictionary<StationNode, ThreadStatus> Threads;
        }

        private List<StationNode> OrderStations()
        {
            var ret = ProcessSplit(StartNode);
            return ret;
        }

        Dictionary<string, PendingJoin> _pendingJoins = [];

        private List<StationNode> ProcessThread(StationNode? pred, StationNode node)
        {
            if (node == EndNode)
                return [];
            else if (node.IsMergePoint() && pred!=null)
            {
                var (joinNode, mergeResult) = ProcessMerge(node, pred, []);
                if (joinNode == null)
                    return [];
                var ret2 = ProcessThread(null, joinNode);
                return [..mergeResult, ..ret2];
            }
            else if (node.IsForkPoint())
            {
                var splitResult = ProcessSplit(node);
                return splitResult;
            }
            else
            {
                List<StationNode> ret = [];

                ret.Add(node);
                var currentNode = node.GetNext();

                while (currentNode.IsPassThru() && currentNode != EndNode)
                {
                    ret.Add(currentNode);
                    currentNode = currentNode.GetNext();
                };

                var ret2 = ProcessThread(ret.Last(), currentNode);
                return [.. ret, .. ret2];

            }

        }
        private (StationNode?, List<StationNode>) ProcessMerge(StationNode node, StationNode pred, List<StationNode> listSoFar)
        {
            if (!_pendingJoins.TryGetValue(node.Id, out var pendingJoin))
            {
                pendingJoin = new PendingJoin() { Id = node, Threads = node.Prev.ToDictionary(n => n, n => new ThreadStatus() { Flagged = false, indexedStationNodes = [] }) };
                _pendingJoins.Add(node.Id, pendingJoin);
            }

            pendingJoin.Threads[pred] = new ThreadStatus() { Flagged = true, indexedStationNodes = listSoFar };

            if (pendingJoin.Threads.Values.All(t => t.Flagged))
            {
                (StationNode?, List<StationNode>) ret = (node, pendingJoin.Threads.Values.OrderByDescending(t => t.indexedStationNodes.Count).SelectMany(t => t.indexedStationNodes).ToList());
                _pendingJoins.Remove(node.Id);

                return ret;
            }

            return (null, []);
        }

        private List<StationNode> ProcessSplit(StationNode node)
        {
            List<List<StationNode>> threads = [];
            
            foreach(var head in node.Next)
            {
                var threadResult = ProcessThread(node, head).ToList();
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
                    station.AddNext(EndNode);
                }
            }

        }

        private StationNode Add(Branch branch, MatchedStop stopPoint, StationNode? prev)
        {
            var node = GetOrCreate(branch.BranchId, stopPoint);

            //var existingPrevs = node.Prev;
            if (prev != null && !node.Follows(prev))
            {
                node.RemovePrev(StartNode);
                //foreach (var p in existingPrevs)
                //{
                //    if (!p.Next.Contains(node))
                //        p.Next.Add(node);
                //}
                node.AddPrev(prev);
            }
            else if (prev == null && node.IsHead()
                && !branch.StopPointSequence.PrevBranchIds.Where(id => id != branch.StopPointSequence.BranchId).Any())
            {
                node.AddPrev(StartNode);
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