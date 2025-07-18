using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]
    public class StopPointTests
    {
        Action<string, string> SaveTestOutput = TestUtils.SaveTestOutput;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StopPointClient _stopPointClient;
        public StopPointTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _stopPointClient = new StopPointClient(_httpClientFactory.CreateClient());
        }

        [Fact]
        public async void TestArrivals()
        {
            var ret = await _stopPointClient.ArrivalsAsync("910GHTRWTM4");
        }

        [Fact]
        public async void TestMode()
        {
            var ret = await _stopPointClient.GetByModeAsync(["tube", "elizabeth-line", "dlr"], null);
            var stopTypes = ret.StopPoints.Select(sp => sp.StopType).Distinct().ToList();

            var stops = ret.StopPoints.Where(sp => new[] { "NaptanMetroStation","NaptanRailStation" }.Contains(sp.StopType) ).ToList();
            var el = stops.Where(sp => sp.Modes.Contains("elizabeth-line")).ToList();
            var dlr = stops.Where(sp => sp.Modes.Contains("dlr")).ToList();
        }

    }
}
