using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.Text.Json;
using tfl_stats.Tfl;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]
    public class LineTests
    {
        Action<string, string> SaveTestOutput = TestUtils.SaveTestOutput;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineClient _client;
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

        [Theory (Skip = "Takes a long time, run manually")]
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
            var t = ret.Where(l => l.ModeName == "elizabeth-line");
        }

        [Fact]
        public async void RouteSequenceAsync()
        {
            // StopPoint contains the list of the stations on the given line in order
            var ret = await _client.RouteSequenceAsync("bakerloo", Direction.Inbound,[Anonymous6.Regular], null);

            var json = JsonConvert.SerializeObject(ret, Formatting.Indented );

            SaveTestOutput("RouteSequence.json", json);

            var stops = ret.StopPointSequences
                .SingleOrDefault(s => s.ServiceType == StopPointSequenceServiceType.Regular, null)
                ?.StopPoint
                .Select((sp, i) => new {i, sp.Id, sp.Name }).ToList() ?? [];
        }

        [Fact]
        public async void ArrivalsAsync()
        {
            // 
            var ret = await _client.ArrivalsAsync(["bakerloo"], "940GZZLUBST", null, null);

            var json = JsonConvert.SerializeObject(ret, Formatting.Indented);

            SaveTestOutput("ArrivalsAsync.json", json);
        }


    }
}