using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;
using tfl_stats.Tfl;

namespace TestTflObjects
{
    [Collection("HttpClientFactory collection")]

    public class LineTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineClient _client;
        public LineTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _client = new LineClient(_httpClientFactory.CreateClient());
        }
        [Fact]
        public async void Test1()
        {
            var ret = await _client.StatusByModeAsync(["tube"], true, null);
        }
    }
}