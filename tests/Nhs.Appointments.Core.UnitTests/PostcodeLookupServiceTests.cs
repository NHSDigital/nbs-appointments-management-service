using Microsoft.Extensions.Options;
using System.Net;
using static Nhs.Appointments.Core.PostcodeLookupService;

namespace Nhs.Appointments.Core.UnitTests
{
    public class PostcodeLookupServiceTests
    {
        [Fact]
        public async Task UseCorrectHttpClient()
        {
            var mockHttpClient = new MockHttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(new PostcodeLookupSearchResponse
            {
                Results = new List<PostcodeLookupSearchResult>
                {
                    new PostcodeLookupSearchResult
                    {
                        SearchScore = 1.0,
                        Latitude = 0.0,
                        Longitude = 0.0,
                    }
                }
            });

            var mockOptions = new Mock<IOptions<PostcodeLookupService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new PostcodeLookupService.Options { ServiceName = "test" });
            var sut = new PostcodeLookupService(mockHttpClientFactory.Object, mockOptions.Object);
            await sut.GeolocationFromPostcode("TS33ST");

            mockHttpClientFactory.Verify(x => x.CreateClient("test"), Times.Once());
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task FindSitesByArea_ThrowsException_WhenApiReturnsNonSuccess(HttpStatusCode status)
        {
            var mockHttpClient = new MockHttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);
            mockHttpClient.EnqueueResponse(status);
            var mockOptions = new Mock<IOptions<PostcodeLookupService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new PostcodeLookupService.Options { ServiceName = "test" });
            var sut = new PostcodeLookupService(mockHttpClientFactory.Object, mockOptions.Object);
            Func<Task> act = () => sut.GeolocationFromPostcode("TS33ST");
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task FindSitesByArea_ThrowsException_WhenApiReturnsZeroResults()
        {
            var mockHttpClient = new MockHttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(new PostcodeLookupSearchResponse
            {
                Results = new List<PostcodeLookupSearchResult>()                
            });

            var mockOptions = new Mock<IOptions<PostcodeLookupService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new PostcodeLookupService.Options { ServiceName = "test" });
            var sut = new PostcodeLookupService(mockHttpClientFactory.Object, mockOptions.Object);
            Func<Task> act = () => sut.GeolocationFromPostcode("TS33ST");
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task FindSitesByArea_ReturnsMostRelevantResult_WhenApiReturnsMultipleResults()
        {
            var mockHttpClient = new MockHttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(new PostcodeLookupSearchResponse
            {
                Results = new List<PostcodeLookupSearchResult>
                {
                    new PostcodeLookupSearchResult
                    {
                        SearchScore = 0.4,
                        Latitude = 1.0,
                        Longitude = 11.0,
                    },
                    new PostcodeLookupSearchResult
                    {
                        SearchScore = 0.6,
                        Latitude = 2.0,
                        Longitude = 22.0,
                    },
                    new PostcodeLookupSearchResult
                    {
                        SearchScore = 0.5,
                        Latitude = 3.0,
                        Longitude = 33.0,
                    }
                }
            });

            var mockOptions = new Mock<IOptions<PostcodeLookupService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new PostcodeLookupService.Options { ServiceName = "test" });
            var sut = new PostcodeLookupService(mockHttpClientFactory.Object, mockOptions.Object);
            var (latitude, longitude) = await sut.GeolocationFromPostcode("TS33ST");
            latitude.Should().Be(2.0);
            longitude.Should().Be(22.0);
        }

    }
}
