using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using tfl_stats.Tfl;
using TflNetworkBuilder;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]
    public class LineTests
    {
        Action<string, string> SaveTestOutput = TestUtils.SaveTestOutput;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineClient _client;
        private static readonly string[] modes = ["tube", "dlr", "elizabeth-line"];
        private static readonly (string,Direction)[] lines = [
            ("bakerloo", Direction.Inbound),
            ("central", Direction.Inbound),
            ("circle", Direction.Inbound),
            ("district", Direction.Inbound),
            ("dlr", Direction.Outbound),
            ("elizabeth", Direction.Inbound),
            ("hammersmith-city", Direction.Inbound),
            ("jubilee", Direction.Outbound),
            ("metropolitan", Direction.Outbound),
            ("northern", Direction.Inbound),
            ("piccadilly", Direction.Inbound),
            ("victoria", Direction.Inbound),
            ("waterloo-city", Direction.Inbound)
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
            foreach (var (line, dir) in lines)
            {
                // StopPoint contains the list of the stations on the given line in order
                var ret = await _client.RouteSequenceAsync(line, dir, [Anonymous6.Regular], null);
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
            //foreach (var (line, dir) in lines)
            var line = "elizabeth";
            var dir = Direction.Inbound;
            {
                // StopPoint contains the list of the stations on the given line in order
                var ret = await _client.RouteSequenceAsync(line, dir, [Anonymous6.Regular], null);

                var branches = ret.StopPointSequences.Select(seq => new Branch(
                    (int)seq.BranchId!,
                    seq.PrevBranchIds.ToList(),
                    seq.NextBranchIds.ToList(),
                    seq.Direction,
                    seq.StopPoint.ToList()
                )).ToList();

                SaveTestOutput($"{line}-BranchAnalysis.json", JsonConvert.SerializeObject(branches, Formatting.Indented));


                Dictionary<string, Station> stations = [];

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
                        indexedStopPoint = new Station() { StationId = sp.StationId };
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

                Queue<Station> indexedStations = [];

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


        [Fact]
        public async void BranchAnalysisAsync2()
        {
            //foreach (var (line, dir) in lines)
                var line = "elizabeth";
            //var line = "dlr";
            var dir = Direction.Inbound;
            {
                // StopPoint contains the list of the stations on the given line in order
                var lineData = await _client.RouteSequenceAsync(line, dir, [Anonymous6.Regular], null);

                //var branches = lineData.StopPointSequences.Select(seq => new Branch(
                //    (int)seq.BranchId!,
                //    seq.PrevBranchIds.ToList(),
                //    seq.NextBranchIds.ToList(),
                //    seq.Direction,
                //    seq.StopPoint.ToList()
                //)).ToList();

                //SaveTestOutput($"{line}-BranchAnalysis.json", JsonConvert.SerializeObject(branches, Formatting.Indented));
                //
                // Not reliable test, because the query returns live status information about the branch
                //
                //TestUtils.VerifyTestOutput($"{line}-BranchAnalysis.json", JsonConvert.SerializeObject(branches, Formatting.Indented));

                StationGraph graph = new();

                //
                // To build the graph, we need to begin by adding all StopPointSequences
                //
                foreach (var stopPointSequence in lineData.StopPointSequences)
                {
                    graph.AddBranch(stopPointSequence);
                }

                var orderedStationList = graph.Construct();

                var testResult = orderedStationList.Select(s => new
                {
                    s.Station.StationId,
                    StopPoints = s.Station.MatchedStop.Select(ms=> new
                    {
                        ms.Name,
                        ms.Id
                    })
                });

                //SaveTestOutput($"{line}-NodeAnalysis.json", JsonConvert.SerializeObject(graph.DumpNodes(), Formatting.Indented));
                //
                // Check that the Nodes have been built correctly
                //
                TestUtils.VerifyTestOutput($"{line}-NodeAnalysis.json", JsonConvert.SerializeObject(graph.DumpNodes(), Formatting.Indented));

                //SaveTestOutput($"{line}-OrderedStationList.json", JsonConvert.SerializeObject(testResult, Formatting.Indented));
                //
                // Check that the order of the Nodes has been built correctly
                //
                TestUtils.VerifyTestOutput($"{line}-OrderedStationList.json", JsonConvert.SerializeObject(testResult, Formatting.Indented));
            }
        }

        [Fact]
        public async void ArrivalsAsync()
        {
            // 
            //var lineData = await _client.ArrivalsAsync(["bakerloo"], "940GZZLUBST", null, null);
            var ret = await _client.ArrivalsAsync(["elizabeth"], "910GHTRWTM4", null, null); // LHR 4

            var json = JsonConvert.SerializeObject(ret, Formatting.Indented);

            SaveTestOutput($"ArrivalsAsync.json", json);
        }


    }

}