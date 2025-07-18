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
    public class PlaceTests
    {
        Action<string, string> SaveTestOutput = TestUtils.SaveTestOutput;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PlaceClient _client;
        public PlaceTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _client = new PlaceClient(_httpClientFactory.CreateClient());
        }

        [Fact]
        public async void TestArrivals()
        {
            var ret = await _client.GetOverlayAsync(1, ["BikePoint"], 1, 1, "51.1", "0.0", 51.1, 0.0);
        }


    }
}

