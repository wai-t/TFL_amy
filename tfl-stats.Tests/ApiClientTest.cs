using Microsoft.Extensions.Logging;
using Moq;
using tfl_stats.Server.Client;
using tfl_stats.Server.Models.JourneyModels;
using tfl_stats.Server.Models.StopPointModels;
using tfl_stats.Server.Models.StopPointModels.Mode;

namespace tfl_stats.Tests
{
    public class ApiClientTest
    {
        private readonly ApiClient _apiClient;

        public ApiClientTest()
        {
            // Arrange
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.tfl.gov.uk/"),
            };
            var mockApiClientLogger = new Mock<ILogger<ApiClient>>();
            _apiClient = new ApiClient(httpClient, mockApiClientLogger.Object);
        }
        [Fact]
        [Trait("Category", "TflApiTests")]
        public async Task TestLineStatusQuery()
        {
            var result = await _apiClient.GetFromApi<List<Server.Models.LineModels.Line>>("Line/Mode/tube/Status");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(11, result.Count);
            Assert.Equal("circle", result[2].Id);
        }

        [Theory]
        [Trait("Category", "TflApiTests")]
        [InlineData("Angel", "Angel Underground Station")]
        [InlineData("Liverpool", "Liverpool Street")]
        public async Task TestStopPoints(string query, string expected)
        {
            var url = $"StopPoint/Search/{Uri.EscapeDataString(query)}?modes=tube";
            var result = await _apiClient.GetFromApi<StopPointSearchResponse>(url);

            Assert.NotNull(result);
            Assert.NotNull(result.Matches.Find(x => x.Name == expected));
        }

        [Theory]
        [Trait("Category", "TflApiTests")]
        [InlineData("1000007", "1000138")] // Angel to Liverpool Street
        public async Task TestJourneyPlanner(string fromIcsCode, string toIcsCode)
        {
            var url = $"Journey/journeyresults/{fromIcsCode}/to/{toIcsCode}?mode=tube";
            var result = await _apiClient.GetFromApi<JourneyResponse>(url);

            Assert.NotNull(result);
            Assert.True(result.Journeys.Count > 0);
            for (var i = 0; i < result.Journeys.Count; i++)
            {
                Assert.NotNull(result.Journeys[i].Legs);
                Assert.True(result.Journeys[i].Legs.Count > 0);
                Assert.Equal(fromIcsCode, result.Journeys[i].Legs[0].DeparturePoint.IcsCode);
                Assert.Equal(toIcsCode, result.Journeys[i].Legs.Last().ArrivalPoint.IcsCode);
            }
        }

        [Fact]
        [Trait("Category", "TflApiTests")]
        public async Task AllStopPoints()
        {
            var url = "StopPoint/Mode/tube";
            var result = await _apiClient.GetFromApi<StopPointModeResponse>(url);
            Assert.NotNull(result);
            var stops = result.StopPoints.Where(sp => sp.StopType == "NaptanMetroStation").ToList();
        }

        // General utility test to investigate API responses
        [Fact]
        [Trait("Category", "TflApiTests")]
        public async Task TestDynamic()
        {
            var url = $"Journey/journeyresults/1000007/to/1000138?mode=tube";
            var result = await _apiClient.GetFromApi<dynamic>(url);
            Assert.NotNull(result);
            var asString = result?.ToString();
        }
    }
}
