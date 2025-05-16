using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Functions;

[MockedFeatureToggle(Flags.MultipleServices, false)]
public class MakeBookingFunctionTests : FeatureToggledTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly Mock<IBookingWriteService> _bookingWriteService = new();
    private readonly Mock<ILogger<MakeBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly MakeBookingFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<MakeBookingRequest>> _validator = new();

    public MakeBookingFunctionTests() : base(typeof(MakeBookingFunctionTests))
    {
        _sut = new MakeBookingFunction(_bookingWriteService.Object, _siteService.Object, _validator.Object,
            _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<MakeBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenAppointmentIsRequested()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(11, 0),
            TimeSpan.FromMinutes(5));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
            new Site("6877d86e-c2df-4def-8508-e1eccf0ea6ba", "Test Site", "Nowhere", "2929292", "15N", "North",
                "Test Board", "Information For Citizen 123", Enumerable.Empty<Accessibility>(),
                new Location("Point", [0, 0])));
        _bookingWriteService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((true, "TEST01"));

        var request = CreateRequest("34e990af-5dc9-43a6-8895-b9123216d699", "2077-01-01 10:30", "COVID", "9999999999",
            "FirstName", "LastName",
            "1958-06-08", "test@tempuri.org", "0123456789", null);

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<MakeBookingResponse>(result.Content);
        response.BookingReference.Should().Be("TEST01");
    }

    [Fact]
    public async Task RunAsync_ReturnsError_WhenSiteDoesNotExist()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Site)null);

        var request = CreateRequest("34e990af-5dc9-43a6-8895-b9123216d699", "2077-01-01 09:30", "COVID", "9999999999",
            "FirstName", "LastName",
            "1958-06-08", "test@tempuri.org", "0123456789", null);

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
        var response = await ReadResponseAsync<BadRequestBody>(result.Content);
        response.message.Should().Be("Site for booking request could not be found");
    }

    [Fact]
    public async Task RunAsync_ReturnsError_WhenAppointmentSlotIsNotAvailable()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
            new Site("6877d86e-c2df-4def-8508-e1eccf0ea6ba", "Test Site", "Nowhere", "2929292", "15N", "North",
                "Test Board", "Information For Citizens 123", Enumerable.Empty<Accessibility>(),
                new Location("Point", [0, 0])));
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(11, 0),
            TimeSpan.FromMinutes(5));

        var request = CreateRequest("34e990af-5dc9-43a6-8895-b9123216d699", "2077-01-01 09:30", "COVID", "9999999999",
            "FirstName", "LastName",
            "1958-06-08", "test@tempuri.org", "0123456789", null);

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
        var response = await ReadResponseAsync<BadRequestBody>(result.Content);
        response.message.Should().Be("The time slot for this booking is not available");
    }

    [Fact]
    public void RunAsync_InvokesBookingService_WithCorrectDetails()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
            new Site("6877d86e-c2df-4def-8508-e1eccf0ea6ba", "Test Site", "Nowhere", "2929292", "15N", "North",
                "Test Board", "Information For Citizens 123", Enumerable.Empty<Accessibility>(),
                new Location("Point", [0, 0])));
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(11, 0),
            TimeSpan.FromMinutes(5));

        _bookingWriteService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((true, "TEST01"));

        var request = CreateRequest("34e990af-5dc9-43a6-8895-b9123216d699", "2077-01-01 10:30", "COVID", "9999999999",
            "FirstName", "LastName",
            "1958-06-08", "test@tempuri.org", "0123456789", null);
        var expectedBooking = new Booking
        {
            Site = "34e990af-5dc9-43a6-8895-b9123216d699",
            Duration = 5,
            Service = "COVID",
            From = new DateTime(2077, 1, 1, 10, 30, 0),
            Status = AppointmentStatus.Booked,
            AttendeeDetails = new AttendeeDetails
            {
                FirstName = "FirstName",
                LastName = "LastName",
                NhsNumber = "9999999999",
                DateOfBirth = new DateOnly(1958, 6, 8)
            },
            ContactDetails =
            [
                new ContactItem { Value = "test@tempuri.org", Type = ContactItemType.Email },
                new ContactItem { Value = "0123456789", Type = ContactItemType.Phone }
            ]
        };
        _sut.RunAsync(request);
        _bookingWriteService.Invocations.Should().HaveCount(1);
        var actualArgument = _bookingWriteService.Invocations[0].Arguments[0];
        actualArgument.Should().BeEquivalentTo(expectedBooking);
    }

    private static HttpRequest CreateRequest(
        string site,
        string from,
        string service,
        string nhsNumber,
        string firstName,
        string lastName,
        string dateOfBirth,
        string email,
        string phoneNumber,
        object additionalData,
        bool emailContactConsent = true,
        bool phoneContactConsent = true)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var dto = new
        {
            site,
            from,
            duration = 5,
            service,
            attendeeDetails =
                new
                {
                    NhsNumber = nhsNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = DateOnly.ParseExact(dateOfBirth, "yyyy-MM-dd")
                },
            contactDetails = new[]
            {
                new { type = "email", value = email }, new { type = "phone", value = phoneNumber }
            },
            additionalData,
            kind = "booked"
        };

        var body = JsonConvert.SerializeObject(dto);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }

    private record BadRequestBody(string message, string property);
}
