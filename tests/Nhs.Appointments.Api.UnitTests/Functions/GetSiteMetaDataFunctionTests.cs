using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSiteMetaDataFunctionTests
{
    private readonly GetSiteMetaDataFunction _sut;
    private readonly Mock<ISiteConfigurationService> _siteConfigurationService = new();
    private readonly Mock<IUserSiteAssignmentService> _userSiteAssignmentService = new();
    private readonly Mock<IValidator<SiteBasedResourceRequest>> _validator = new();
    private readonly Mock<IRequestAuthenticatorFactory> _authenticatorFactory = new();
    private readonly Mock<IRequestAuthenticator> _authenticator = new();
    private readonly Mock<ILogger<GetSiteMetaDataFunction>> _logger = new();

    public GetSiteMetaDataFunctionTests()
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
        _sut = new GetSiteMetaDataFunction(_userSiteAssignmentService.Object, _siteConfigurationService.Object, _validator.Object, _authenticatorFactory.Object, _logger.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<SiteBasedResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccess_WhenRequestedSiteIsConfigured()
    {
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationOrDefaultAsync("123")).ReturnsAsync(new SiteConfiguration { InformationForCitizen = "Test data" });
        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(200);
        var actualResponse = await ReadResponseAsync<GetSiteMetaDataResponse>(result.Content);
        actualResponse.Site.Should().Be("123");
        actualResponse.AdditionalInformation.Should().Be("Test data");
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenRequestedSiteIsNotConfigured()
    {
        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(404);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString($"?site=123");
        request.Headers.Add("Authorization", "Test 123");
        return request;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
