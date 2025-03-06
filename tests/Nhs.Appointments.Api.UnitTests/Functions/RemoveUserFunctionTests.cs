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

public class RemoveUserFunctionTests
{
    private readonly Mock<ILogger<RemoveUserFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly RemoveUserFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();

    private readonly Mock<IUserService> _userService = new();

    public RemoveUserFunctionTests()
    {
        _sut = new RemoveUserFunction(_userService.Object, new RemoveUserRequestValidator(),
            _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenUserIsRemoved()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        _userContextProvider.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);
        _userService.Setup(service => service.RemoveUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(true));

        var request = CreateRequest("test.user@nhs.net", "34e990af-5dc9-43a6-8895-b9123216d699");

        var result = ParseResponse(await _sut.RunAsync(request, functionContext: null));
        result.StatusCode.Should().Be(200);

        var response = await ReadResponseAsync<RemoveUserResponse>(result.Content);
        response.Site.Should().Be("34e990af-5dc9-43a6-8895-b9123216d699");
        response.User.Should().Be("test.user@nhs.net");

        _userService.Verify(
            service => service.RemoveUserAsync("test.user@nhs.net", "34e990af-5dc9-43a6-8895-b9123216d699"),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenUserDoesNotHaveRolesAtTheSite()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        _userContextProvider.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);
        _userService.Setup(service => service.RemoveUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(false, "User has no roles at site"));

        var request = CreateRequest("test.user@nhs.net", "34e990af-5dc9-43a6-8895-b9123216d699");

        var result = ParseResponse(await _sut.RunAsync(request, functionContext: null));
        result.StatusCode.Should().Be(404);

        _userService.Verify(
            service => service.RemoveUserAsync("test.user@nhs.net", "34e990af-5dc9-43a6-8895-b9123216d699"),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequestResponse_WhenRequestIsInvalid()
    {
        var request = CreateRequest("", "34e990af-5dc9-43a6-8895-b9123216d699");

        var result = ParseResponse(await _sut.RunAsync(request, functionContext: null));
        result.StatusCode.Should().Be(400);

        _userService.Verify(service => service.RemoveUserAsync("", "34e990af-5dc9-43a6-8895-b9123216d699"),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequestResponse_WhenUserIsTryingToRemoveThemself()
    {
        const string testUser = "test.user@nhs.net";
        var userPrincipal = UserDataGenerator.CreateUserPrincipal(testUser);

        _userContextProvider.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);

        var request = CreateRequest(testUser, "34e990af-5dc9-43a6-8895-b9123216d699");

        var result = ParseResponse(await _sut.RunAsync(request, functionContext: null));
        result.StatusCode.Should().Be(400);

        _userService.Verify(service => service.RemoveUserAsync(testUser, "34e990af-5dc9-43a6-8895-b9123216d699"),
            Times.Never);
    }

    private static HttpRequest CreateRequest(string user, string site)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = $"{{ \"user\":\"{user}\"," +
                   $"\"site\":\"{site}\"" +
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
