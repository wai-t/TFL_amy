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
    public class JourneyTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private JourneyClient _client;

        public JourneyTests(HttpClientFactoryFixture fixture)
        {
            _httpClientFactory = fixture.Services.GetRequiredService<IHttpClientFactory>();
            _client = new JourneyClient(_httpClientFactory.CreateClient());
        }

        [Fact]
        public async void Test1()
        {
            var ret = await _client.JourneyResultsAsync("1000007", "1000138", null, null, null, null, null, null, ["tube"],
                [], null, null, null, null, null, null, null, null, [],null,null,null,null,null,null,null,null,null,null,
                null, null);
            Assert.NotNull(ret);
            Assert.NotEmpty(ret.Journeys);
        }
    }
}
