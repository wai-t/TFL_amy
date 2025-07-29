using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;
using TflNetworkBuilder;

namespace TestTflNetworkBuilder
{
    [Collection("HttpClientFactory collection")]
    public class NetworkBuilderTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineClient _client;
        private static readonly string[] modes = ["tube", "dlr", "elizabeth-line"];
        private static readonly (string, Direction)[] lines = [
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

        public NetworkBuilderTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _client = new LineClient(_httpClientFactory.CreateClient());
        }

        [Fact]
        public async void BranchAnalysisAsync2()
        {
            foreach (var (line, dir) in lines)
            //var line = "elizabeth";
            //var line = "dlr";
            //var dir = Direction.Inbound;
            {
                // StopPoint contains the list of the stations on the given line in order
                var lineData = await _client.RouteSequenceAsync(line, dir, [Anonymous6.Regular], null);

                StationGraph graph = new(lineData);

                var stationNodeDtoList = graph.OrderedNodes.Select(s => s.ToDto());

                TestUtils.SaveTestOutput($"{line}-StationNodeDtoList.json", JsonConvert.SerializeObject(stationNodeDtoList, Formatting.Indented));

                TestUtils.SaveTestOutput($"{line}-BranchesList.json", JsonConvert.SerializeObject(graph.Branches, Formatting.Indented));


            }
        }
        [Fact]
        public async void TestGeometry()
        {
            //foreach (var (line, dir) in lines)
            var line = "dlr";
            //var line = "dlr";
            var dir = Direction.Outbound;
            {
                // StopPoint contains the list of the stations on the given line in order
                var lineData = await _client.RouteSequenceAsync(line, dir, [Anonymous6.Regular], null);

                StationGraph graph = new(lineData);

                var stationNodeDtoList = graph.OrderedNodes;

                var geomAnalyser = new GeometryAnalyser(graph.Branches, []);

            }
        }
    }
}
