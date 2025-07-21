using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NuGet.Frameworks;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.Text.Json;
using tfl_stats.Tfl;
using Xunit.Sdk;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]
    public class LineTests
    {
        Action<string, string> SaveTestOutput = TestUtils.SaveTestOutput;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineClient _client;
        private static readonly string[] modes = ["tube", "dlr", "elizabeth-line"];
        private static readonly string[] lines = [
            "bakerloo", "central","circle", "district", "dlr", "elizabeth",
            "hammersmith-city","jubilee","metropolitan","northern",
            "piccadilly","victoria","waterloo-city"
        ];
        private static Dictionary<string, string> StopPointIdMap = [];

        public LineTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _client = new LineClient(_httpClientFactory.CreateClient());
        }
        [Fact]
        public async void MetaModesAsync()
        {
            var ret = await _client.MetaModesAsync();

            File.WriteAllText("MetaModes.json", JsonConvert.SerializeObject(ret, Formatting.Indented));
        }

        [Theory(Skip = "Takes a long time, run manually")]
        [InlineData("tube")]
        [InlineData("dlr")]
        [InlineData("elizabeth-line")]
        [InlineData("overground")]
        [InlineData("bus")]
        [InlineData("national-rail")]
        [InlineData("cable-car")]
        public async void StatusByModeAsync(string mode)
        {
            var ret = await _client.StatusByModeAsync([mode], true, null);

            SaveTestOutput($"StatusByMode-{mode}.json", JsonConvert.SerializeObject(ret, Formatting.Indented));
        }

        [Fact]
        public async void GetByModeAsync()
        {
            // return all lines for the given modes
            ICollection<Line> ret = await _client.GetByModeAsync(["tube",
                "dlr","elizabeth-Line","overground"/*,"bus","national-rail","cable-car"*/]);

            SaveTestOutput($"GetByModeAsync.json", JsonConvert.SerializeObject(ret, Formatting.Indented));

            var lines = ret.Select(l => new
            {
                l.Id,
                l.Name,
                l.ModeName,
                l.LineStatuses
            }).ToList();
        }

        [Fact]
        public async void StopPointsAsync()
        {
            // return StopPoints for the given lines, but doesn't order them
            var ret = await _client.StopPointsAsync("bakerloo", null);
        }

        [Fact]
        public async void RouteAsync()
        {
            // It looks a route is origin/destination/direction for a given line.
            // So on one line you can have multiple routes. For Elizabeth Line,
            // you can have Abbey Wood to Reading in both directions (inbound and outbound)
            // and then there is Liverpool St to Shenfield, etc.
            var ret = await _client.RouteAsync([Anonymous3.Regular]);
            ret = ret.Where(l => modes.Contains(l.ModeName)).ToList();
            SaveTestOutput("RouteAsync.json", JsonConvert.SerializeObject(ret, Formatting.Indented));
            var t = ret.Where(l => l.ModeName == "elizabeth-line");
        }

        [Fact]
        public async void RouteSequenceAsync()
        {
            foreach (var line in lines)
            {
                // StopPoint contains the list of the stations on the given line in order
                var ret = await _client.RouteSequenceAsync(line, Direction.Inbound, [Anonymous6.Regular], null);
                var stops = ret.StopPointSequences
                    .Select(seq => new
                    {
                        seq.ServiceType,
                        seq.BranchId,
                        seq.PrevBranchIds,
                        seq.NextBranchIds,
                        seq.Direction,
                        StopPoints = seq.StopPoint.Select(sp => new { sp.Id, sp.Name })
                    });
                var json = JsonConvert.SerializeObject(stops, Formatting.Indented);
                SaveTestOutput($"{line}-RouteSequence.json", json);
            }
        }

        [Fact]
        public async void BranchAnalysisAsync()
        {
            //foreach (var line in lines)
            var line = "elizabeth";
            {
                // StopPoint contains the list of the stations on the given line in order
                var ret = await _client.RouteSequenceAsync(line, Direction.Inbound, [Anonymous6.Regular], null);

                var branches = ret.StopPointSequences.Select(seq => new Branch(
                    (int)seq.BranchId!,
                    seq.PrevBranchIds.ToList(),
                    seq.NextBranchIds.ToList(),
                    seq.Direction,
                    seq.StopPoint.ToList()
                )).ToList();

                SaveTestOutput($"{line}-BranchAnalysis.json", JsonConvert.SerializeObject(branches, Formatting.Indented));


                Dictionary<string, IndexedStation> stations = [];

                var all = ret.StopPointSequences.SelectMany(
                    branch => branch.StopPoint,
                    (branch, sp) => (
                            StationId: sp.ParentId ?? sp.Id,
                            StopPoint: sp,
                            branch.BranchId
                            )
                        );

                foreach (var sp in all)
                {
                    if (!stations.TryGetValue(sp.StationId, out var indexedStopPoint))
                    {
                        indexedStopPoint = new IndexedStation() { StationId = sp.StationId };
                        stations[sp.StationId] = indexedStopPoint;
                    }
                    if (!indexedStopPoint.MatchedStop.Any(ms => ms.Id == sp.StopPoint.Id))
                        indexedStopPoint.MatchedStop.Add(sp.StopPoint); 
                    indexedStopPoint.BranchIds.Add((int)sp.BranchId!);
                }

                var branchStack = new List<Branch>(
                    branches.Where(b =>
                        b.PrevBranchIds.Count == 0
                        || b.PrevBranchIds.Contains(b.BranchId)  // Deal with circle line
                        || b.NextBranchIds.Contains(b.BranchId))
                    );

                while (branchStack.Count > 0)
                {
                    var maxCount = branchStack.Max(b => b.StopPoints.Count);
                    var longestBranches = branchStack.Where(b => b.StopPoints.Count == maxCount).ToList();
                    branchStack.RemoveAll(b => b.StopPoints.Count == maxCount);
                    foreach ( var branch in longestBranches)
                    {
                        int index = 1;
                        foreach (var stopPoint in branch.StopPoints)
                        {
                            string stationId = stopPoint.ParentId ?? stopPoint.Id;

                            if (index > stations[stationId].Index)
                            {
                                stations[stationId].Index = index;
                                index++;
                            }
                            else
                                index = stations[stationId].Index + 1;
                        }
                        foreach (var nextBranchId in branch.NextBranchIds)
                        {
                            var nextBranch = branches.FirstOrDefault(b => b.BranchId == nextBranchId);
                            if (nextBranch != null && !branchStack.Contains(nextBranch))
                                branchStack.Add(nextBranch);
                        }
                    }

                }

                Assert.Empty(stations.Values.Where(s => s.Index == int.MinValue));

                var orderedStations = stations.Values
                    .OrderBy(sp => sp.Index)
                    .ToList();

                var json = JsonConvert.SerializeObject(orderedStations, Formatting.Indented);

                SaveTestOutput($"{line}-StationsAnalysis.json", json);

                Queue<IndexedStation> indexedStations = [];

                while (orderedStations.Count > 0)
                {
                    var stationBatch = orderedStations.TakeWhile(s => s.BranchIds.Count == 1).ToList();
                    if (stationBatch.Count() > 0)
                    {
                        var grouping = stationBatch
                            .GroupBy(s => s.BranchIds[0])
                            .OrderByDescending(g => g.Count())
                            .SelectMany(g => g.ToList());
                        foreach (var station in grouping)
                        {
                            indexedStations.Enqueue(station);
                        }
                    }
                    else
                    {
                        indexedStations.Enqueue(orderedStations.Take(1).First());
                    }
                }
            }
        }

        public class IndexedStation
        {
            public required string StationId { get; set; }
            public int Index { get; set; } = int.MinValue;
            public List<int> BranchIds { get; set; } = [];

            public List<MatchedStop> MatchedStop { get; set; } = [];

            public override bool Equals(object? obj)
            {
                var right = obj as IndexedStation;
                return right != null && StationId == right.StationId;
            }

            public override int GetHashCode()
            {
                return StationId.GetHashCode();
            }
        }

        public class IndexedStationNode
        {
            public string Id => Station.StationId;
            public required IndexedStation Station {get; init;}
            public List<IndexedStationNode> Next { get; } = [];
            public List<IndexedStationNode> Prev { get; } = [];

            public void AddNext(IndexedStationNode nextNode)
            {
                if (!Next.Contains(nextNode))
                    Next.Add(nextNode);
                if (!nextNode.Prev.Contains(this))
                    nextNode.Prev.Add(this);
            }

            public void AddPrev(IndexedStationNode prevNode)
            {
                if (!Prev.Contains(prevNode))
                    Prev.Add(prevNode);
                if (!prevNode.Next.Contains(this))
                    prevNode.Next.Add(this);
            }

            public override bool Equals(object? obj)
            {
                var right = obj as IndexedStationNode;
                return right != null && Id == right.Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        internal class IndexedBranch
        {
            private readonly StopPointSequence _sequence;

            public IndexedBranch(StopPointSequence sequence)
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
        internal class IndexedStationGraph
        {
            private HashSet<IndexedStationNode> _nodes = [];
            private HashSet<IndexedBranch> _branches = [];

            public IndexedStationNode StartNode { get; } = new() { Station = new() { StationId = "START" } };
            public IndexedStationNode EndNode { get; } = new() { Station = new() { StationId = "END" } };

            public void AddBranch(StopPointSequence stopPointSequence)
            {
                Assert.True(_branches.Add(new IndexedBranch(stopPointSequence)));
            }

            public List<IndexedStationNode> Construct()
            {
                BuildStationNetwork();
                var ret = OrderStations();
                return ret;
            }

            public class ThreadStatus
            {
                public bool Flagged = false;
                public List<IndexedStationNode> indexedStationNodes = new();
            }
            public class PendingJoin
            {
                public required IndexedStationNode Id;
                public required Dictionary<IndexedStationNode, ThreadStatus> Threads;
            }

            public class PendingSplit
            {
                public required IndexedStationNode Id;
                public Dictionary<IndexedStationNode, List<IndexedStationNode>> threads = [];
            }

            private List<IndexedStationNode> OrderStations()
            {
                var ret = ProcessSplit(StartNode);
                return ret;

            }

            Dictionary<string, PendingJoin> _pendingJoins = [];

            private List<IndexedStationNode> ProcessThread(IndexedStationNode pred, IndexedStationNode node)
            {
                List<IndexedStationNode> ret = [];
                if (node.Prev.Count>1)
                {
                    ret = ProcessJoin(node, pred, []) ;
                    if (!ret.Any())
                        return ret;
                }
                if (node.Next.Count>1)
                {
                    ret = ProcessSplit(node);
                    return ret;
                }

                var currentNode = node;

                do
                {
                    ret.Add(currentNode);
                    currentNode = currentNode.Next.Single();
                }
                while (currentNode.Prev.Count == 1 && currentNode.Next.Count == 1);

                if (currentNode.Prev.Count>1)
                {
                    var joinres = ProcessJoin(currentNode, ret.Last(), ret);
                    if (joinres.Count==0)
                        return []; // means there's another branch waiting to be merged
                    ret = joinres;
                    currentNode = joinres.Last();
                }

                if (currentNode == EndNode)
                    return ret;

                if (currentNode != EndNode)
                {
                    var r = ProcessSplit(currentNode).ToList();
                    ret = ret.Concat(r).ToList();
                }

                return ret;
            }
            private List<IndexedStationNode> ProcessJoin(IndexedStationNode node, IndexedStationNode pred, List<IndexedStationNode> listSoFar)
            {
                if (!_pendingJoins.TryGetValue(node.Id, out var pendingJoin))
                {
                    pendingJoin = new PendingJoin() { Id = node, Threads = node.Prev.ToDictionary(n => n, n => new ThreadStatus() { Flagged=false, indexedStationNodes = [] }) };
                    _pendingJoins.Add(node.Id, pendingJoin);
                }

                pendingJoin.Threads[pred] = new ThreadStatus() { Flagged=true, indexedStationNodes=listSoFar };

                if (pendingJoin.Threads.Values.All(t=>t.Flagged))
                {
                    var ret = pendingJoin.Threads.Values.OrderByDescending(t => t.indexedStationNodes.Count).SelectMany(t=>t.indexedStationNodes).ToList();
                    ret.Add(node);
                    _pendingJoins.Remove(node.Id);
                    if ( node.Next.Count==1)
                    {
                        var nextNode = node.Next.Single();
                        if (nextNode.Prev.Count > 1)
                            ret = ProcessJoin(nextNode, node, ret);
                    }
                    return ret;
                }

                return [];
            }

            private List<IndexedStationNode> ProcessSplit(IndexedStationNode node)
            {
                var threads = node.Next.Select(head => ProcessThread(node, head)).ToList();

                threads = threads.OrderBy(t => t.Count).ToList();
                var ret = threads.SelectMany(t => t).ToList();
                ret.Insert(0, node);
                return ret;
            }
            private void BuildStationNetwork()
            {
                Stack<IndexedBranch> workQueue = new(_branches.Where(b => !b.StopPointSequence.PrevBranchIds.Any()));
                var remainingBranches = _branches.Except(workQueue);

                while (workQueue.Any())
                {
                    IndexedStationNode? prevStation = null;
                    var branch = workQueue.Pop();

                    foreach (var stopPoint in branch.StopPointSequence.StopPoint)
                    {
                        prevStation = Add(branch, stopPoint, prevStation);
                    }

                    foreach (var branchId in branch.StopPointSequence.NextBranchIds)
                    {
                        var nextBranch = remainingBranches.SingleOrDefault(b => b.BranchId == branchId);
                        if (nextBranch != null)
                            workQueue.Push(nextBranch);
                    }
                }
                foreach (var station in _nodes)
                {
                    if (!station.Next.Any())
                    {
                        station.Next.Add(EndNode);
                        EndNode.Prev.Add(station);
                    }
                }

            }

            private IndexedStationNode Add(IndexedBranch branch, MatchedStop stopPoint, IndexedStationNode? prev)
            {
                var node = GetOrCreate(branch.BranchId, stopPoint);

                var existingPrevs = node.Prev;
                if (prev!=null && !existingPrevs.Contains(prev))
                {
                    existingPrevs.Remove(StartNode);
                    StartNode.Next.Remove(node);
                    foreach (var p in existingPrevs)
                    {
                        if (!p.Next.Contains(node)) 
                            p.Next.Add(node);
                    }
                    node.Prev.Add(prev);
                    prev.Next.Add(node);
                }
                else if (prev == null && !node.Prev.Any() && !branch.StopPointSequence.PrevBranchIds.Any())
                {
                    node.Prev.Add(StartNode);
                    StartNode.Next.Add(node);
                }
                else
                {
                    Assert.True(true);
                }

                return node;

            }

            private IndexedStationNode GetOrCreate(int branchId, MatchedStop stopPoint)
            {
                var stationId = stopPoint.ParentId ?? stopPoint.Id;

                var entry = _nodes.SingleOrDefault(n => n.Id == stationId);
                if (entry is null)
                {
                    entry = new IndexedStationNode() { Station = new IndexedStation() { 
                        StationId = stationId,
                        BranchIds = [branchId],
                        MatchedStop = [stopPoint] } };
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

            public record PrintableNode (string Id, List<String> Prev, List<string> Next);

            public List<PrintableNode> DumpNodes()
            {
                List<PrintableNode> output = [];
                foreach (var node in _nodes)
                {
                    output.Add(new PrintableNode(
                        node.Id,
                        node.Prev.Select(n => n.Id).ToList(),
                        node.Next.Select(n => n.Id).ToList()
                        )
                    );
                }
                return output;
            }

        }

        [Fact]
        public async void BranchAnalysisAsync2()
        {
            //foreach (var line in lines)
            var line = "elizabeth";
            {
                // StopPoint contains the list of the stations on the given line in order
                var ret = await _client.RouteSequenceAsync(line, Direction.Inbound, [Anonymous6.Regular], null);

                var branches = ret.StopPointSequences.Select(seq => new Branch(
                    (int)seq.BranchId!,
                    seq.PrevBranchIds.ToList(),
                    seq.NextBranchIds.ToList(),
                    seq.Direction,
                    seq.StopPoint.ToList()
                )).ToList();

                SaveTestOutput($"{line}-BranchAnalysis.json", JsonConvert.SerializeObject(branches, Formatting.Indented));

                IndexedStationGraph graph = new();

                foreach (var stopPointSequence in ret.StopPointSequences)
                {
                    graph.AddBranch(stopPointSequence);
                }


                SaveTestOutput($"{line}-NodeAnalysis.json", JsonConvert.SerializeObject(graph.DumpNodes(), Formatting.Indented));

                var ordered = graph.Construct().Select(s => new
                {
                    s.Station.StationId,
                    StopPoints = s.Station.MatchedStop.Select(ms=> new
                    {
                        ms.Name,
                        ms.Id
                    })
                });
                SaveTestOutput($"{line}-OrderedStationList.json", JsonConvert.SerializeObject(ordered, Formatting.Indented));
            }
        }

        [Fact]
        public async void ArrivalsAsync()
        {
            // 
            //var ret = await _client.ArrivalsAsync(["bakerloo"], "940GZZLUBST", null, null);
            var ret = await _client.ArrivalsAsync(["elizabeth"], "910GHTRWTM4", null, null); // LHR 4

            var json = JsonConvert.SerializeObject(ret, Formatting.Indented);

            SaveTestOutput($"ArrivalsAsync.json", json);
        }


    }

}