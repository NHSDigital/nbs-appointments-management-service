using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class ProposePotentialUserFunctionTests
{
    private readonly Mock<ILogger<ProposePotentialUserFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly ProposePotentialUserFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();

    private readonly Mock<IUserService> _userService = new();

    public ProposePotentialUserFunctionTests() =>
        _sut = new ProposePotentialUserFunction(_userService.Object, new ProposePotentialUserRequestValidator(),
            _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);

    [Fact]
    public async Task ProposePotentialUser_ShouldReturnOk()
    {
        _userService.Setup(service => service.GetUserIdentityStatusAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ProposePotentialUserResponse
            {
                ExtantInMya = true,
                ExtantInIdentityProvider = true,
                MeetsWhitelistRequirements = true,
                IdentityProvider = IdentityProvider.NhsMail
            });

        var request = CreateRequest("34e990af-5dc9-43a6-8895-b9123216d699", "test.user@nhs.net");

        var result = ParseResponse(await _sut.RunAsync(request));
        result.StatusCode.Should().Be(200);

        var response = await ReadResponseAsync<ProposePotentialUserResponse>(result.Content);
        response.ExtantInMya.Should().BeTrue();
        response.ExtantInIdentityProvider.Should().BeTrue();
        response.MeetsWhitelistRequirements.Should().BeTrue();
        response.IdentityProvider.Should().Be(IdentityProvider.NhsMail);

        _userService.Verify(
            service => service.GetUserIdentityStatusAsync("34e990af-5dc9-43a6-8895-b9123216d699", "test.user@nhs.net"),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequestResponse_WhenRequestIsInvalid()
    {
        var request = CreateRequest("34e990af-5dc9-43a6-8895-b9123216d699", "");

        var result = ParseResponse(await _sut.RunAsync(request));
        result.StatusCode.Should().Be(400);

        _userService.Verify(service => service.RemoveUserAsync("34e990af-5dc9-43a6-8895-b9123216d699", ""),
            Times.Never);
    }

    private static HttpRequest CreateRequest(string siteId, string userId)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = $"{{ \"userId\":\"{userId}\"," +
                   $"\"siteId\":\"{siteId}\"" +
                   $"}}";
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }

    private static ContentResult ParseResponse(IActionResult rawResponse)
    {
        rawResponse.Should().NotBeNull();
        if (rawResponse is not ContentResult response)
        {
            throw new NullReferenceException();
        }

        return response;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        response.Should().NotBeNull();
        if (response is null)
        {
            throw new NullReferenceException();
        }

        var body = await new StringReader(response).ReadToEndAsync();
        var payload = JsonConvert.DeserializeObject<TRequest>(body);
        if (payload is null)
        {
            throw new Exception("Could not deserialise response body as request type.");
        }

        return payload;
    }
}
