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
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;

public class ConfirmProvisionalBookingFunctionTests
{
    private readonly Mock<IBookingWriteService> _bookingService = new();
    private readonly Mock<ILogger<ConfirmProvisionalBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly ConfirmProvisionalBookingFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<ConfirmBookingRequest>> _validator = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IBookingQueryService> _bookingQueryService = new();

    public ConfirmProvisionalBookingFunctionTests()
    {
        _sut = new ConfirmProvisionalBookingFunction(
            _bookingService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _featureToggleHelper.Object,
            _siteService.Object,
            _bookingQueryService.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<ConfirmBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Theory]
    [InlineData("a", "b")]
    [InlineData()]
    public async Task RunAsync_CallsConfirmProvisionalBooking_WhenJointBookingsDisabled(params string[] relatedBookings)
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(false);
        _bookingService.Setup(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()))
            .ReturnsAsync(BookingConfirmationResult.Success);
        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(new Booking
            {
                Reference = "booking-ref",
                Site = "test-site"
            });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    "test-site",
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = CreateRequest(relatedBookings);

        await _sut.RunAsync(request);
        _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings")), Times.Once);
        _bookingService.Setup(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()));
        _bookingService.Verify(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_CallsConfirmProvisionalBooking_WhenNoChildBookings()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(true);
        _bookingService.Setup(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()))
            .ReturnsAsync(BookingConfirmationResult.Success);
        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(new Booking
            {
                Reference = "booking-ref",
                Site = "test-site"
            });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    "test-site",
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = CreateRequest([]);

        await _sut.RunAsync(request);
        _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings")), Times.Once);
        _bookingService.Setup(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()));
        _bookingService.Verify(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("a", "b")]
    [InlineData("a")]
    public async Task RunAsync_CallsConfirmProvisionalBookingWithChildren_WhenJointBookingsEnabled(params string[] relatedBookings)
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(true);
        _bookingService.Setup(x => x.ConfirmProvisionalBookings(It.IsAny<string[]>(), It.IsAny<IEnumerable<ContactItem>>())).ReturnsAsync(BookingConfirmationResult.Success);
        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(new Booking
            {
                Reference = "booking-ref",
                Site = "test-site"
            });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    "test-site",
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = CreateRequest(relatedBookings);

        await _sut.RunAsync(request);
        _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings")), Times.Once);
        _bookingService.Setup(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()));
        _bookingService.Verify(x => x.ConfirmProvisionalBookings(It.IsAny<string[]>(), It.IsAny<IEnumerable<ContactItem>>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenSiteHasBeenDeleted()
    {

        _bookingQueryService.Setup(x => x.GetBookingByReference(It.IsAny<string>()))
            .ReturnsAsync(new Booking
            {
                Reference = "booking-ref",
                Site = "test-site"
            });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var request = CreateRequest([]);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(404);

        _bookingService.Verify(x => x.ConfirmProvisionalBookings(It.IsAny<string[]>(), It.IsAny<IEnumerable<ContactItem>>()), Times.Never);
    }

    private static HttpRequest CreateRequest(
        string[] relatedBookings)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var dto = new
        {
            contactDetails = new List<object>() 
            {
                new { type = "Phone", value = "phonenumber" },
                new { type = "Email", value = "email@test.com" }
            },
            relatedBookings,
            bookingToReschedule = string.Empty
        };

        var body = JsonConvert.SerializeObject(dto);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
