using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Results;
using Nhs.Appointments.Api.Auth;
using System.Security.Claims;

namespace Nhs.Appointments.Api.Tests.Functions;

public class BaseApiFunctionTests
{
    private readonly TestableBaseApiFunction _sut;
    private readonly Mock<IValidator<string>> _validator = new();
    private readonly Mock<IRequestAuthenticatorFactory> _authenticatorFactory = new();
    private readonly TestLogger _logger = new();
    private readonly Mock<IRequestAuthenticator> _authenticator = new();    

    public BaseApiFunctionTests()
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, "username"),
            new Claim(ClaimTypes.NameIdentifier, "userId"),            
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _authenticator.Setup(x => x.AuthenticateRequest(It.IsAny<string>())).ReturnsAsync(claimsPrincipal);
        _authenticatorFactory.Setup(x => x.CreateAuthenticator(It.IsAny<string>())).Returns(_authenticator.Object);
        _sut = new TestableBaseApiFunction(_validator.Object, _authenticatorFactory.Object, _logger);
    }

    [Fact]
    public async Task RunAsync_ReturnUnauthorized_WhenAuthHeaderIsMissing()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var response = await _sut.RunAsync(request) as ContentResult;
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RunAsync_ReturnUnauthorized_WhenAuthenticationFails()
    {
        var request = GetDefaultRequest();
        var unauthedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _authenticator.Setup(x => x.AuthenticateRequest(It.IsAny<string>())).ReturnsAsync(unauthedPrincipal);
        var response = await _sut.RunAsync(request) as ContentResult;
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RunAsync_ReturnUnauthorized_WhenSchemeUnrecognised()
    {
        var request = GetDefaultRequest();
        _authenticator.Setup(x => x.AuthenticateRequest(It.IsAny<string>())).ThrowsAsync(new NotSupportedException());
        var response = await _sut.RunAsync(request) as ContentResult;
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RunAsync_LogsError_WhenUnhandleExceptionReceivedOnHandle()
    {
        var exception = new SystemException("Test Exception");
        var request = GetDefaultRequest();
        
        _sut.HandleDelegate = (r, l) => { throw exception; };            

        await _sut.RunAsync(request);

        _logger.LogEntries.First().LogLevel.Should().Be(LogLevel.Error);
        _logger.LogEntries.First().Message.Should().Be("Test Exception");
        _logger.LogEntries.First().Exception.Should().Be(exception);
    }

    [Fact]
    public async Task RunAsync_LogsError_WhenUnhandleExceptionReceivedOnReadingRequest()
    {
        var exception = new SystemException("Test Exception");
        var request = GetDefaultRequest();

        _sut.ReadRequestDelegate = (http) => { throw exception; };

        await _sut.RunAsync(request);

        _logger.LogEntries.First().LogLevel.Should().Be(LogLevel.Error);
        _logger.LogEntries.First().Message.Should().Be("Test Exception");
        _logger.LogEntries.First().Exception.Should().Be(exception);
    }

    [Fact]
    public async Task RunAsync_LogsError_WhenUnhandleExceptionReceivedOnValidatingRequest()
    {
        var exception = new SystemException("Test Exception");
        var request = GetDefaultRequest();
        
        _sut.ValidateRequestDelegate = (req) => { throw exception; };

        await _sut.RunAsync(request);

        _logger.LogEntries.First().LogLevel.Should().Be(LogLevel.Error);
        _logger.LogEntries.First().Message.Should().Be("Test Exception");
        _logger.LogEntries.First().Exception.Should().Be(exception);
    }

    [Fact]
    public async Task RunAsync_ReturnBadRequest_WhenRequestCannotBeParsed()
    {
        var exception = new SystemException("Test Exception");
        var request = GetDefaultRequest();

        _sut.ReadRequestDelegate = (req) => (false, "");

        var result = await _sut.RunAsync(request) as ContentResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequest_WhenValidationFails()
    {
        var request = GetDefaultRequest();
        _sut.ValidateRequestDelegate = (req) => new[] { new ErrorMessageResponseItem() };

        var result = await _sut.RunAsync(request) as ContentResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RunAsync_ReturnNonSuccess_WhenHandleCompletesWithFailure()
    {
        var request = GetDefaultRequest();
        _sut.HandleDelegate = (req, log) => ApiResult<string>.Failed(System.Net.HttpStatusCode.NotFound, "Not Found");

        var result = await _sut.RunAsync(request) as ContentResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task RunAsync_ReturnSuccess_WhenHandleCompletesWithSuccess()
    {
        var request = GetDefaultRequest();
        _sut.HandleDelegate = (req, log) => ApiResult<string>.Success("hooray");

        var result = await _sut.RunAsync(request) as ContentResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ValidateRequest_ReturnsEmptyArray_WhenRequestIsValid()
    {
        var validationResult = new ValidationResult();
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(validationResult));

        var result = await _sut.TestValidateRequest("test");
        result.Should().BeEmpty();            
    }

    [Fact]
    public async Task ValidateRequest_ReturnsValidationErrors_WhenRequestIsInvalid()
    {
        var validationResult = new ValidationResult();
        validationResult.Errors = new List<ValidationFailure>()
        {
            new ValidationFailure("sites", "Provide a site"),
            new ValidationFailure("from", "Provide a from date"),
            new ValidationFailure("until", "Provide an until date")
        };
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(validationResult));

        var result = await _sut.TestValidateRequest("test");
        result.Should().HaveCount(3);
        result.SingleOrDefault(x => x.Property == "sites" && x.Message == "Provide a site").Should().NotBeNull();
        result.SingleOrDefault(x => x.Property == "from" && x.Message == "Provide a from date").Should().NotBeNull();
        result.SingleOrDefault(x => x.Property == "until" && x.Message == "Provide an until date").Should().NotBeNull();

    }

    private HttpRequest GetDefaultRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers.Add("Authorization", "Test 1243");
        return request;
    }
}

internal class TestableBaseApiFunction : BaseApiFunction<string, string>
{
    public TestableBaseApiFunction(
        IValidator<string> validator,
        IRequestAuthenticatorFactory authenticatorFactory,
        ILogger logger) : base(validator, authenticatorFactory, logger)
    {
    }

    public Task<IEnumerable<ErrorMessageResponseItem>> TestValidateRequest(string request)
    {
        return base.ValidateRequest(request);
    }

    protected override Task<ApiResult<string>> HandleRequest(string request, ILogger logger)
    {
        if (HandleDelegate is null)
            return Task.FromResult(ApiResult<string>.Success(""));

        return Task.FromResult(HandleDelegate(request, logger));
    }

    protected override Task<(bool requestRead, string request)> ReadRequestAsync(HttpRequest req)
    {
        if (ReadRequestDelegate is null)
            return Task.FromResult((true, ""));
        return Task.FromResult(ReadRequestDelegate(req));
    }

    protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(string request)
    {
        if(ValidateRequestDelegate is null)
            return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
        return Task.FromResult(ValidateRequestDelegate(request));
    }

    public Func<string, ILogger, ApiResult<string>> HandleDelegate { get; set; }

    public Func<HttpRequest, (bool requestRead, string request)> ReadRequestDelegate { get; set; }

    public Func<string, IEnumerable<ErrorMessageResponseItem>> ValidateRequestDelegate { get; set; }
}

