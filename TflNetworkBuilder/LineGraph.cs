using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using tfl_stats.Tfl;
using Xunit;
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method


namespace TflNetworkBuilder
{

    public class LineGraph
    {
        private readonly HashSet<StationNode> _nodes = [];
        private readonly HashSet<Branch> _branches = [];
        private List<StationNode> _orderedNodes = [];
        //private HashSet<StationLink> _stationLinks = [];
        private string _lineId = string.Empty;

        private StationNode START_NODE { get; } = new() { Station = new() { StationId = "START" } };
        private StationNode END_NODE { get; } = new() { Station = new() { StationId = "END" } };

        public List<StationNode> OrderedNodes => _orderedNodes;
        public List<Branch> Branches => [.. _branches];

        public LineGraph(RouteSequence sequenceData)
        {
            _lineId = sequenceData.LineId;
            //
            // To build the graph, we need to begin by adding all StopPointSequences
            //
            foreach (var stopPointSequence in sequenceData.StopPointSequences)
            {
                AddBranch(stopPointSequence);
            }
            Construct();
        }
        //
        // For each StopPointSequence in the line's RouteSequenceAsync query,
        // call this method to add it to the graph. This needs to be done
        // before constructing the network
        //
        private void AddBranch(StopPointSequence stopPointSequence)
        {
            Assert.True(_branches.Add(new Branch(stopPointSequence)));
        }

        //
        // Main Entry Point to process the Station Network and produce a stable ordering.
        //
        private void Construct()
        {
            BuildStationNetwork();
            _orderedNodes = OrderStations();
        }


        private List<StationNode> OrderStations()
        {
            var ret = ProcessSequence(null, START_NODE, []).Append(END_NODE);
            foreach (var (i, node) in ret.Select((node, i) => (i, node)))
            {
                node.Station.Index = i;
            }
            return ret.ToList();
        }

        private List<StationNode> ProcessSequence(StationNode? pred, StationNode node, List<StationNode> broughtForward)
        {
            if (node == END_NODE)
                // This is the last recursion.  Return everything we have collected
                return broughtForward;
            else if (node.IsMergePoint() && pred!=null)
            {
                var ret = ProcessMerge(node, pred, broughtForward);
                return ret;
            }
            else if (node.IsForkPoint())
            {
                var splitResult = ProcessFork(node, broughtForward);
                return splitResult;
            }
            else // Passthru
            {
                return ProcessChain(node, broughtForward);
            }

        }

        private List<StationNode> ProcessChain(StationNode node, List<StationNode> broughtForward)
        {
            List<StationNode> ret = broughtForward;

            ret.Add(node);
            var currentNode = node.GetNext();

            while (currentNode.IsPassThru() && currentNode != END_NODE)
            {
                ret.Add(currentNode);
                currentNode = currentNode.GetNext();
            };

            var ret2 = ProcessSequence(ret.Last(), currentNode, ret);
            return ret2;
        }

        private MergeProcessor _mergeProcessor = new();
        private List<StationNode> ProcessMerge(StationNode node, StationNode pred, List<StationNode> stationsOnBranch)
        {
           if (_mergeProcessor.ProcessMerge(node, pred, ref stationsOnBranch))
            {
                return ProcessSequence(null, node, stationsOnBranch);
            }
           return [];
        }

        private List<StationNode> ProcessFork(StationNode node, List<StationNode> broughtForward)
        {
            List<List<StationNode>> threads = [];
            
            foreach(var head in node.Next)
            {
                var threadResult = ProcessSequence(node, head, []).ToList();
                threads.Add(threadResult);
            }

            threads = threads.OrderBy(t => t.Count).ToList();
            var ret = threads.SelectMany(t => t).ToList();
            ret.Insert(0, node);
            return [..broughtForward, ..ret];
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

        public string LinearOutput()
        {
            var branches = _branches.OrderBy(b => b.BranchId).ToArray();

            HashSet<Branch> activeBranches = [];

            StringBuilder line = new();
            foreach (var stationNode in _orderedNodes)
            {
                StringBuilder rec = new();
                var station = stationNode.Station;
                foreach (var branch in branches)
                {
                    if (branch.StartsAt(stationNode))
                    {
                        activeBranches.Add(branch);
                    }
                    if (branch.Visits(stationNode))
                    {
                        rec.Append("=");
                    }
                    else if (activeBranches.Contains(branch))
                    {
                        rec.Append("|");
                    }
                    else
                    {
                        rec.Append(" ");
                    }
                    if (branch.EndsAt(stationNode))
                    {
                        activeBranches.Remove(branch);
                    }
                }
                line.Append(rec.ToString());
                line.AppendLine(stationNode.StationId);
            }

            return line.ToString();
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

        private LineGraph()
        {

        }

        //
        // Only used for testing
        //
        public static LineGraph StationGraphFromTestData(HashSet<StationNode> nodes)
        {

            var sg = new LineGraph();
            foreach (var node in nodes)
            {
                if (!node.Prev.Any()) node.AddPrev(sg.START_NODE);
                if (!node.Next.Any()) node.AddNext(sg.END_NODE);
            }
            sg._nodes.Clear();
            sg._nodes.UnionWith(nodes);
            
            sg._orderedNodes = sg.OrderStations();

            return sg;
        }

    }


}