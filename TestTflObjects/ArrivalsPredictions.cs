using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [Fact(Skip ="This test overloads the TfL server and returns HTTP 429 errors frequently")]
        public async Task TestGenerateArrivalPredictions()
        {
            var lines = (await _lineClient.GetByModeAsync(["tube", "dlr", "elizabeth-line"]))
                .Select(l => l.Id).ToList();

            List<IEnumerable<LineArrivals>> groupedPredictions = await ExperimentalArrivals.GenerateArrivalPredictionsAsync(lines, _lineClient);

            SaveTestOutput("ArrivalsPredictions.json", JsonConvert.SerializeObject(groupedPredictions, Formatting.Indented));

        }

    }


}
