using Microsoft.Extensions.Options;
using System.Net;

namespace Nhs.Appointments.Core.UnitTests
{
    public class SiteSearchServiceTests
    {
        [Fact]
        public async Task SiteSearchUseCorrectHttpClient()
        {
            var mockHttpClient = new MockHttpClient();
            var mockSiteStore= new Mock<ISiteStore>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(
                new SiteSearchService.SiteSearchResponse
                {
                    Sites = new List<SiteSearchService.SiteSearchResponseEntry>
                        {
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 1,
                                SiteName = "test",
                                SiteAddress = "test address"
                            }
                        }
                });            

            var mockOptions = new Mock<IOptions<SiteSearchService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new SiteSearchService.Options { ServiceName = "test" });
            var sut = new SiteSearchService(mockSiteStore.Object, mockHttpClientFactory.Object, mockOptions.Object);
            await sut.FindSitesByArea(0, 0, 0, 0);
            
            mockHttpClientFactory.Verify(x => x.CreateClient("test"), Times.Once());
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task FindSitesByArea_ThrowsException_WhenApiReturnsNonSuccess(HttpStatusCode status)
        {
            var mockHttpClient = new MockHttpClient();
            var mockSiteStore = new Mock<ISiteStore>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);
            mockHttpClient.EnqueueResponse(status);
            var mockOptions = new Mock<IOptions<SiteSearchService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new SiteSearchService.Options { ServiceName = "test" });
            var sut = new SiteSearchService(mockSiteStore.Object, mockHttpClientFactory.Object, mockOptions.Object);
            Func<Task> act = () => sut.FindSitesByArea(0, 0, 0, 0);
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task FindSitesByArea_ReturnsSites_WhenReceivedFromApi()
        {
            var mockHttpClient = new MockHttpClient();
            var mockSiteStore = new Mock<ISiteStore>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(
                new SiteSearchService.SiteSearchResponse
                {
                    Sites = new List<SiteSearchService.SiteSearchResponseEntry>
                        {
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 1,
                                SiteName = "alpha",
                                SiteAddress = "alpha address"
                            },
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 2,
                                SiteName = "beta",
                                SiteAddress = "beta address"
                            }
                        }
                });

            var mockOptions = new Mock<IOptions<SiteSearchService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new SiteSearchService.Options { ServiceName = "test" });
            var sut = new SiteSearchService(mockSiteStore.Object, mockHttpClientFactory.Object, mockOptions.Object);
            var results = await sut.FindSitesByArea(0, 0, 0, 0);
            var expectedResults = new Site[]
            {
                new Site("1", "alpha", "alpha address"),
                new Site("2", "beta", "beta address")
            };
            results.Should().BeEquivalentTo(expectedResults);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task GetSiteByIdAsync_ThrowsException_WhenApiReturnsNonSuccess(HttpStatusCode status)        
        {
            var mockHttpClient = new MockHttpClient();
            var mockSiteStore = new Mock<ISiteStore>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);
            mockHttpClient.EnqueueResponse(status);
            var mockOptions = new Mock<IOptions<SiteSearchService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new SiteSearchService.Options { ServiceName = "test" });
            var sut = new SiteSearchService(mockSiteStore.Object, mockHttpClientFactory.Object, mockOptions.Object);
            Func<Task> act = () => sut.GetSiteByIdAsync("test");
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task GetSiteByIdAsync_ReturnsNullWhenSearchReturnsInvalidResults_WhenApiReturnsNonSuccess()
        {
            var mockHttpClient = new MockHttpClient();
            var mockSiteStore = new Mock<ISiteStore>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(
                new SiteSearchService.SiteSearchResponse
                {
                    Sites = new List<SiteSearchService.SiteSearchResponseEntry>
                        {
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 1,
                                SiteName = "alpha",
                                SiteAddress = "alpha address"
                            },
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 2,
                                SiteName = "beta",
                                SiteAddress = "beta address"
                            }
                        }
                });

            var mockOptions = new Mock<IOptions<SiteSearchService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new SiteSearchService.Options { ServiceName = "test" });
            var sut = new SiteSearchService(mockSiteStore.Object, mockHttpClientFactory.Object, mockOptions.Object);
            var results = await sut.GetSiteByIdAsync("test");
            results.Should().BeNull();
        }

        [Fact]
        public async Task GetSiteByIdAsync_ReturnsCorrectSite_WhenMultipleSitesReturned()
        {
            var expectedSite = new Site("2", "beta", "beta address");
            var mockSiteStore = new Mock<ISiteStore>();
            var mockHttpClient = new MockHttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient("test")).Returns(mockHttpClient.Client);

            mockHttpClient.EnqueueJsonResponse(
                new SiteSearchService.SiteSearchResponse
                {
                    Sites = new List<SiteSearchService.SiteSearchResponseEntry>
                        {
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 1,
                                SiteName = "alpha",
                                SiteAddress = "alpha address"
                            },
                            new SiteSearchService.SiteSearchResponseEntry
                            {
                                UnitId = 2,
                                SiteName = "beta",
                                SiteAddress = "beta address"
                            }
                        }
                }); ;

            var mockOptions = new Mock<IOptions<SiteSearchService.Options>>();
            mockOptions.Setup(x => x.Value).Returns(new SiteSearchService.Options { ServiceName = "test" });
            var sut = new SiteSearchService(mockSiteStore.Object, mockHttpClientFactory.Object, mockOptions.Object);
            var results = await sut.GetSiteByIdAsync("2");
            results.Should().BeEquivalentTo(expectedSite);
        }
    }    

}