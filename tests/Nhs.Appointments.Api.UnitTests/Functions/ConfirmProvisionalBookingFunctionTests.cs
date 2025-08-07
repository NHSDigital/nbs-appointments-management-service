using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
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

    public ConfirmProvisionalBookingFunctionTests()
    {
        _sut = new ConfirmProvisionalBookingFunction(_bookingService.Object, _validator.Object,
            _userContextProvider.Object, _logger.Object, _metricsRecorder.Object, _featureToggleHelper.Object);
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

        var request = CreateRequest(relatedBookings);

        await _sut.RunAsync(request);
        _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings")), Times.Once);
        _bookingService.Setup(x => x.ConfirmProvisionalBooking(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>()));
        _bookingService.Verify(x => x.ConfirmProvisionalBookings(It.IsAny<string[]>(), It.IsAny<IEnumerable<ContactItem>>()), Times.Once);
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
