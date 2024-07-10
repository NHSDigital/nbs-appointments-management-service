using System.Net;
using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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
    private readonly MakeBookingFunction _sut;
    private readonly Mock<IBookingsService> _bookingService = new();
    private readonly Mock<ISiteConfigurationService> _siteConfigurationService = new();
    private readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<MakeBookingRequest>> _validator = new();
    private readonly Mock<ILogger<MakeBookingFunction>> _logger = new();

    public MakeBookingFunctionTests()
    {
        _sut = new MakeBookingFunction(_bookingService.Object, _siteConfigurationService.Object, _availabilityCalculator.Object,  _validator.Object, _userContextProvider.Object, _logger.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<MakeBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenAppointmentIsRequested()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        _bookingService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((true, "TEST01"));
        
        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "SessionHolder","9999999999", "FirstName", "LastName", "1958-06-08");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = ReadResponseAsync<MakeBookingResponse>(result.Content);
        response.Result.BookingReference.Should().Be("TEST01");
    }
    
    [Fact]
    public async Task RunAsync_Returns500_WhenBookingServiceFails()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        _bookingService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((false, string.Empty));

        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "SessionHolder", "9999999999", "FirstName", "LastName", "1958-06-08");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(500);        
    }
    
    [Fact]
    public async Task MakeBookingService_ReturnsError_WhenRequestedSiteIsNotConfigured()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ThrowsAsync(new CosmosException("Resource not found", HttpStatusCode.NotFound, 0, "1", 1));
        
        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "Default","9999999999", "FirstName", "LastName", "1958-06-08");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
        var response = ReadResponseAsync<BadRequestBody>(result.Content);
        response.Result.message.Should().Be("The requested site is not configured for appointments");
    }
    
    [Fact]
    public async Task RunAsync_ReturnsError_WhenRequestedServiceIsNotConfiguredForSite()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "OTHER_SERVICE", "OTHER_SERVICE");
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        
        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "Default","9999999999", "FirstName", "LastName", "1958-06-08");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
        var response = ReadResponseAsync<BadRequestBody>(result.Content);
        response.Result.message.Should().Be("The requested service is not available");
    }

    [Fact]
    public async Task RunAsync_ReturnsError_WhenAppointmentSlotIsNotAvailable()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");    
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        
        var request = CreateRequest("1001", "2077-01-01 09:30", "COVID", "Default","9999999999", "FirstName", "LastName", "1958-06-08");

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
        var response = ReadResponseAsync<BadRequestBody>(result.Content);
        response.Result.message.Should().Be("The time slot for this booking is not available");
    }

    [Fact]
    public void RunAsync_InvokesBookingService_WithCorrectDetails()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");
        
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        _bookingService.Setup(x => x.MakeBooking(It.IsAny<Booking>())).ReturnsAsync((true, "TEST01"));
        
        var request = CreateRequest("1001", "2077-01-01 10:30", "COVID", "SessionHolder","9999999999", "FirstName", "LastName", "1958-06-08");
        var expectedBooking = new Booking
        {
            Site = "1001",
            Duration = 5,
            Service = "COVID",
            SessionHolder = "SessionHolder",
            From = new DateTime(2077, 1, 1, 10, 30, 0),
            AttendeeDetails = new Core.AttendeeDetails
            {
                FirstName = "FirstName",
                LastName = "LastName",
                NhsNumber = "9999999999",
                DateOfBirth = new DateOnly(1958,6,8)
            }
        };
        _sut.RunAsync(request);
        _bookingService.Invocations.Should().HaveCount(1);
        var actualArgument = _bookingService.Invocations.First().Arguments.First();
        actualArgument.Should().BeEquivalentTo(expectedBooking);
    }

    private static HttpRequest CreateRequest(string site, string from, string service, string sessionHolder, string nhsNumber, string firstName, string lastName, string dateOfBirth)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = $"{{ \"from\":\"{from}\"," +
                   $"\"service\":\"{service}\"," +
                   $"\"site\":\"{site}\"," +
                   $"\"sessionHolder\":\"{sessionHolder}\"," +
                   $"\"attendeeDetails\":" +
                   $"{{\"nhsNumber\":\"{nhsNumber}\"," +
                   $"\"firstName\":\"{firstName}\"," +
                   $"\"lastName\":\"{lastName}\"," +
                   $"\"dateOfBirth\":\"{dateOfBirth}\"}}" +
                   $"}}";
        request.Body =  new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Add("Authorization", "Test 123");
        return request;
    }
    
    private static SiteConfiguration CreateSiteConfiguration(string siteId, string serviceCode, string serviceName)
    {
        return new SiteConfiguration
        {
            Site = siteId,
            ServiceConfiguration = new List<ServiceConfiguration> { new (serviceCode, serviceName, 5, true) }
        };
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }

    private record BadRequestBody(string message, string property);
}
