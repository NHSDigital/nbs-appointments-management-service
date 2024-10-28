using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class MakeBookingFunctionTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly MakeBookingFunction _sut;
    private readonly Mock<IBookingsService> _bookingService = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<MakeBookingRequest>> _validator = new();
    private readonly Mock<ILogger<MakeBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    public MakeBookingFunctionTests()
    {
        _sut = new MakeBookingFunction(_bookingService.Object, _validator.Object, _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<MakeBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenAppointmentIsRequested()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10,0), new TimeOnly(11,0), TimeSpan.FromMinutes(5));        
        _bookingService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((true, "TEST01"));
        
        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "9999999999", "FirstName", "LastName", "1958-06-08", "test@tempuri.org", "0123456789");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = ReadResponseAsync<MakeBookingResponse>(result.Content);
        response.Result.BookingReference.Should().Be("TEST01");
    }
    
    [Fact]
    public async Task RunAsync_ReturnsError_WhenAppointmentSlotIsNotAvailable()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(11, 0), TimeSpan.FromMinutes(5));
        
        var request = CreateRequest("1001", "2077-01-01 09:30", "COVID","9999999999", "FirstName", "LastName", "1958-06-08", "test@tempuri.org", "0123456789");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
        var response = ReadResponseAsync<BadRequestBody>(result.Content);
        response.Result.message.Should().Be("The time slot for this booking is not available");
    }

    [Fact]
    public void RunAsync_InvokesBookingService_WithCorrectDetails()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(11, 0), TimeSpan.FromMinutes(5));
        
        _bookingService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((true, "TEST01"));
        
        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "9999999999", "FirstName", "LastName", "1958-06-08", "test@tempuri.org", "0123456789");
        var expectedBooking = new Booking
        {
            Site = "1001",
            Duration = 5,
            Service = "COVID",
            From = new DateTime(2077, 1, 1, 10, 30, 0),
            AttendeeDetails = new Core.AttendeeDetails
            {
                FirstName = "FirstName",
                LastName = "LastName",
                NhsNumber = "9999999999",
                DateOfBirth = new DateOnly(1958,6,8)
            },
            ContactDetails = [
            new Core.ContactItem
            {
                Value = "test@tempuri.org",
                Type = "email"
            },
            new Core.ContactItem
            {
                Value = "0123456789",
                Type = "phone"
            }
            ]
        };
        _sut.RunAsync(request);
        _bookingService.Invocations.Should().HaveCount(1);
        var actualArgument = _bookingService.Invocations.First().Arguments.First();
        actualArgument.Should().BeEquivalentTo(expectedBooking);
    }

    private static HttpRequest CreateRequest(string site, string from, string service, string nhsNumber, string firstName, string lastName, string dateOfBirth, string email, string phoneNumber, bool emailContactConsent = true, bool phoneContactConsent = true)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var dto = new MakeBookingRequest(site, from, 5, service,
            new Models.AttendeeDetails(nhsNumber, firstName, lastName, dateOfBirth),
            [
                new Models.ContactItem ("email", email ),
                new Models.ContactItem ("phone", phoneNumber)
            ]);

        var body = JsonConvert.SerializeObject(dto);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Add("Authorization", "Test 123");
        return request;
    }       

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }

    private record BadRequestBody(string message, string property);
}
