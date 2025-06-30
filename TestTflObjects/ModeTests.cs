using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]
    public class ModeTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ModeClient _client;

        public ModeTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _client = new ModeClient(_httpClientFactory.CreateClient());
        }

        [Fact]
        public async Task Test1()
        {
            var predictions = await _client.ArrivalsAsync("tube", null);
            var sortByVehicle = predictions.OrderBy(p => p.ExpectedArrival).OrderBy(p => p.VehicleId);
        }


    }
}
