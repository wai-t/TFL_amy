using System.Net.Http.Headers;
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
        private List<StationNode> _orderedNodes = [];

        private StationNode START_NODE { get; } = new() { Station = new() { StationId = "START" } };
        private StationNode END_NODE { get; } = new() { Station = new() { StationId = "END" } };

        //
        // For each StopPointSequence in the line's RouteSequenceAsync query,
        // call this method to add it to the graph. This needs to be done
        // before constructing the network
        //
        public void AddBranch(StopPointSequence stopPointSequence)
        {
            Assert.True(_branches.Add(new Branch(stopPointSequence)));
        }

        //
        // Main Entry Point to process the Station Network and produce a stable ordering.
        //
        public List<StationNode> Construct()
        {
            BuildStationNetwork();
            _orderedNodes = OrderStations();
            return _orderedNodes;
        }


        private List<StationNode> OrderStations()
        {
            var ret = ProcessSequence(null, START_NODE);
            return ret;
        }

        private List<StationNode> ProcessSequence(StationNode? pred, StationNode node)
        {
            if (node == END_NODE)
                return []; // Do we need this? Is it ever called?
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
            else // Passthru
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
        // returns tuple
        private (bool,List<StationNode>) ProcessMerge(StationNode node, StationNode pred, List<StationNode> stationsOnBranch)
        {
            if (!_pendingMerges.TryGetValue(node.StationId, out var pendingJoin))
            {
                pendingJoin = new PendingMerge() {
                    StationAtMergeOfBranch = node,
                    MergingBranches = node.Prev.ToDictionary(n => n, n => new BranchStatus() { Complete = false, BranchNodes = [] }) 
                };
                _pendingMerges.Add(node.StationId, pendingJoin);
            }

            pendingJoin.MergingBranches[pred] = new BranchStatus() {
                Complete = true,
                BranchNodes = stationsOnBranch 
            };

            if (pendingJoin.MergingBranches.Values.All(t => t.Complete))
            {
                 var ret = (true, pendingJoin.MergingBranches.Values
                    .OrderByDescending(t => t.BranchNodes.Count)
                    .SelectMany(t => t.BranchNodes).ToList());
                _pendingMerges.Remove(node.StationId);

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

        //
        // This is the Kahn Algorithm for walking a DAG
        //
        private void BuildStationNetwork()
        {
            Stack<Branch> workStack = new(
                _branches.Where(
                    b => !b.StopPointSequence.PrevBranchIds.Any() // branch has no previous branch
                         || b.StopPointSequence.PrevBranchIds.Contains((int)b.StopPointSequence.BranchId!) // branch is a circle
                         )
                );

            var remainingBranches = _branches.Except(workStack); // all the branches not in workStack

            while (workStack.Any())
            {
                StationNode? prevStation = null;
                var branch = workStack.Pop();

                foreach (var stopPoint in branch.StopPointSequence.StopPoint)
                {
                    if (prevStation != null && stopPoint.Id == branch.StopPointSequence.StopPoint.First().Id)
                        break; // break the Circle Line, before the second time we hit Edgware Rd
                    prevStation = AddStation(branch, stopPoint, prevStation);
                }

                foreach (var branchId in branch.StopPointSequence.NextBranchIds
                    .Where(i => i != branch.StopPointSequence.BranchId)) // Needed to deal with Circle line which points to itself
                {
                    var nextBranch = remainingBranches.SingleOrDefault(b => b.BranchId == branchId);
                    if (nextBranch != null)
                        workStack.Push(nextBranch);
                    //
                    // Bug?? We should remove nextBranch from remainingBranches
                    //

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

        private StationNode AddStation(Branch branch, MatchedStop stopPoint, StationNode? prev)
        {
            var node = GetOrCreateStation(branch.BranchId, stopPoint);

            // Build the next and prev links
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

        private StationNode GetOrCreateStation(int branchId, MatchedStop stopPoint)
        {
            var stationId = stopPoint.ParentId ?? stopPoint.Id;

            var entry = _nodes.SingleOrDefault(n => n.StationId == stationId);
            if (entry is null)
            {
                // first time we have seen this StationId
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
                // We have seen this StationId before
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
        // private classes to monitor state
        //

        // TODO refactor into a separate class
        //
        // When Ordering, maintain a list of all merges that are
        // waiting to be completed. A merge is completed when all
        // of the branches feeding into it are complete. Before
        // that, it is pending
        //
        Dictionary<string, PendingMerge> _pendingMerges = [];
        private class PendingMerge
        {
            public required StationNode StationAtMergeOfBranch;
            public required Dictionary<StationNode, BranchStatus> MergingBranches; // Key = Penultimate station on merging branch.
        }

        private class BranchStatus
        {
            public bool Complete = false;               // Complete is set true whenever the merging branch is complete.
            public List<StationNode> BranchNodes = [];  // List of Stations on the merging branch. These will be collected
                                                        // together when the merge is complete.
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
                    node.StationId,
                    [.. node.Station.MatchedStop.Select(s => $"{s.Id} {s.ParentId} {s.Name}")],
                    [.. node.Prev.Select(n => n.StationId)],
                    [.. node.Next.Select(n => n.StationId)],
                    node.Station.BranchIds
                ));
            }
            return output;
        }

    }


}