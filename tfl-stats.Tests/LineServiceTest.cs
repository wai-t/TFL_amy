using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using tfl_stats.Server.Client;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models.LineModels;
using tfl_stats.Server.Services;

namespace tfl_stats.Tests
{
    public class LineServiceTest
    {
        private readonly Mock<ApiClient> _mockApiClient;
        private readonly Mock<IOptions<AppSettings>> _mockOptions;
        private readonly Mock<ILogger<LineService>> _mockLogger;
        private readonly LineService _lineService;
        public LineServiceTest()
        {
            _mockOptions = new Mock<IOptions<AppSettings>>();
            var appsettings = new AppSettings
            {
                appId = "123",
                appKey = "cb52c92815b94cabb22449624d95e007",
                baseUrl = "https://api.tfl.gov.uk/"
            };
            _mockOptions.Setup(o => o.Value).Returns(appsettings);

            _mockLogger = new Mock<ILogger<LineService>>();
            var mockApiClientLogger = new Mock<ILogger<ApiClient>>();

            var mockHttpClinet = new HttpClient();
            _mockApiClient = new Mock<ApiClient>(mockHttpClinet, mockApiClientLogger.Object);


            _lineService = new LineService(_mockApiClient.Object,
                                           _mockOptions.Object,
                                           _mockLogger.Object);
        }

        [Fact]
        public async Task TestGetLine()
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
            Assert.Single((System.Collections.IEnumerable)result.Data);
            Assert.Equal("Jubilee", result.Data[0].Name);
            Assert.Equal("Minor Delays", result.Data[0].LineStatuses[0].StatusSeverityDescription);
        }
    }
}
