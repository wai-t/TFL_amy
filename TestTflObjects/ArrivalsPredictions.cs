using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]
    public class ArrivalsPredictions
    {
        Action<string, string> SaveTestOutput = TestUtils.SaveTestOutput;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineClient _lineClient;

        public ArrivalsPredictions(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _lineClient = new LineClient(_httpClientFactory.CreateClient());
        }

        //
        // a route is a list of stations visited on a line. Each line can have many routes
        [Fact]
        public async Task TestBuildIndexedStops()
        {
            var lines = (await _lineClient.GetByModeAsync(["tube", "dlr", "elizabeth-line"]))
                .Select(l => l.Id).ToList();

            List<Task<(string, IEnumerable<OrderedStation>)>> tasks = [];
            foreach (var line in lines)
            {
                tasks.Add( Task.Run(async () => (line, await ExperimentalArrivals.BuildIndexedStopsForLineAsync(line, _lineClient))) );
            }

            var lineStationLists = await Task.WhenAll([.. tasks]);

            foreach (var (line, stations) in lineStationLists)
            {
                SaveTestOutput($"IndexedStops-{line}.json", JsonConvert.SerializeObject(stations, Formatting.Indented));
            }

        }

        [Fact]
        public async Task TestGenerateArrivalPredictions()
        {
            var lines = (await _lineClient.GetByModeAsync(["tube", "dlr", "elizabeth-line"]))
                .Select(l => l.Id).ToList();

            List<IEnumerable<LineArrivals>> groupedPredictions = await ExperimentalArrivals.GenerateArrivalPredictionsAsync(lines, _lineClient);

            SaveTestOutput("ArrivalsPredictions.json", JsonConvert.SerializeObject(groupedPredictions, Formatting.Indented));

        }

    }


}
