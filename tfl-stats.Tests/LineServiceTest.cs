using Microsoft.Extensions.Logging;
using Moq;
using tfl_stats.Server.Client;
using tfl_stats.Server.Models.LineModels;
using tfl_stats.Server.Services;

namespace tfl_stats.Tests
{
    public class LineServiceTest
    {
        private readonly Mock<ILogger<LineService>> _mockLogger;
        private readonly Mock<ApiClient> _mockApiClient;
        private readonly LineService _lineService;
        public LineServiceTest()
        {
            _mockLogger = new Mock<ILogger<LineService>>();

            _mockApiClient = new Mock<ApiClient>(MockBehavior.Strict, new HttpClient(), Mock.Of<ILogger<ApiClient>>());

            _lineService = new LineService(_mockApiClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task TestLineData()
        {
            var testLines = new List<Line>
        {
            new Line
            {
                Id = "jubilee",
                Name = "Jubilee",
                ModeName = "tube",
                LineStatuses = new List<LineStatus>
                {
                    new LineStatus
                    {
                        Id = 0,
                        StatusSeverity = 9,
                        StatusSeverityDescription = "Minor Delays",
                        Reason = "Jubilee Line: Minor delays due to train cancellations"
                    }
                },
                ServiceTypes = new List<LineServiceTypeInfo>
                {
                    new LineServiceTypeInfo
                    {
                        Name = "Regular",
                        Uri = "/Line/Route?ids=Jubilee&serviceTypes=Regular"
                    },
                    new LineServiceTypeInfo
                    {
                        Name = "Night",
                        Uri = "/Line/Route?ids=Jubilee&serviceTypes=Night"
                    }
                },
                Crowding = new Crowding()
            }
        };

            _mockApiClient
                .Setup(api => api.GetFromApi<List<Line>>(It.IsAny<string>(), "GetLine"))
                .ReturnsAsync(testLines);

            var result = await _lineService.GetLine();

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Jubilee", result.Data[0].Name);
            Assert.Equal("Minor Delays", result.Data[0].LineStatuses[0].StatusSeverityDescription);
        }

        [Fact]
        public async Task TestEmptyLineList()
        {
            _mockApiClient
                .Setup(api => api.GetFromApi<List<Line>>(It.IsAny<string>(), "GetLine"))
                .ReturnsAsync(new List<Line>());

            var result = await _lineService.GetLine();

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task TestNullLineList()
        {
            List<Line> nullLine = null;
            _mockApiClient
                .Setup(api => api.GetFromApi<List<Line>>(It.IsAny<string>(), "GetLine"))
                .ReturnsAsync(nullLine);

            var result = await _lineService.GetLine();

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
    }
}
