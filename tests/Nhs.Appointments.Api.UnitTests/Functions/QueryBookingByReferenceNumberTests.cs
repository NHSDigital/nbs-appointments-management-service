using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Json;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Api.Tests.Functions;

public class QueryBookingByReferenceNumberTests
{
    private readonly Mock<IBookingQueryService> _bookingQueryService = new();
    private readonly Mock<ILogger<QueryBookingByReferenceFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    private readonly QueryBookingByReferenceFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<QueryBookingByReferenceRequest>> _validator = new();
    private readonly Mock<ISiteService> _siteService = new();

    public QueryBookingByReferenceNumberTests()
    {
        _sut = new QueryBookingByReferenceFunction(
            _bookingQueryService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _siteService.Object);

        _validator.Setup(
                x => x.ValidateAsync(It.IsAny<QueryBookingByReferenceRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new ValidationResult()));
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_AndBookingWhenFound()
    {
        const string bookingRef = "ABC123";
        const string site = "TEST01";
        var booking = new Booking
        {
            Reference = bookingRef,
            From = DateTime.Now.AddDays(1),
            Duration = 5,
            AttendeeDetails = new AttendeeDetails
            {
                FirstName = "John", LastName = "Bloggs", NhsNumber = "1234567890"
            },
            Site = site
        };

        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(booking);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                site,
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                 new("test_acces/one", "true")
                },
                new Location("Coords", [1.234, 5.678]),
                null,
                false,
                string.Empty
            ));

        var request = new QueryBookingByReferenceRequest(bookingRef, site);
        var httpRequest = CreateRequest(request.site);

        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);

        var response = await ReadResponseAsync<Booking>(result.Content);
        response.Should().BeEquivalentTo(booking);
        response.Reference.Should().Be(bookingRef);
        response.Site.Should().Be(site);
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenReferenceAndSiteDoNotMatch()
    {
        const string bookingRef = "ABC123";
        var booking = new Booking
        {
            Reference = bookingRef,
            From = DateTime.Now.AddDays(1),
            Duration = 5,
            AttendeeDetails = new AttendeeDetails
            {
                FirstName = "John", LastName = "Bloggs", NhsNumber = "1234567890"
            },
            Site = "TEST01"
        };

        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(booking);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                "Test Site",
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                 new("test_acces/one", "true")
                },
                new Location("Coords", [1.234, 5.678]),
                null,
                false,
                string.Empty
            ));

        var request = new QueryBookingByReferenceRequest(bookingRef, "TEST03");
        var httpRequest = CreateRequest(request.site);

        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenSiteHasBeenSoftDeleted()
    {
        const string bookingRef = "ABC123";
        var booking = new Booking
        {
            Reference = bookingRef,
            From = DateTime.Now.AddDays(1),
            Duration = 5,
            AttendeeDetails = new AttendeeDetails
            {
                FirstName = "John",
                LastName = "Bloggs",
                NhsNumber = "1234567890"
            },
            Site = "TEST01"
        };

        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(booking);

        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(null as Site);
        
        var request = new QueryBookingByReferenceRequest(bookingRef, "TEST03");
        var httpRequest = CreateRequest(request.site);

        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(404);
    }

    private static HttpRequest CreateRequest(string site)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers.Append("Authorization", "Test 123");
        request.QueryString = new QueryString($"?site={site}");
        return request;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var deserializerSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new ShortTimeOnlyJsonConverter(),
                new ShortDateOnlyJsonConverter(),
                new NullableShortDateOnlyJsonConverter()
            },
        };
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body, deserializerSettings);
    }
}
