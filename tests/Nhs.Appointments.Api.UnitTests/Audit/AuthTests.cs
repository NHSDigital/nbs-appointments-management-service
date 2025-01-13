using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Audit.Services;

namespace Nhs.Appointments.Api.Tests.Audit;

public class AuthTests
{
    private readonly Mock<IAuditWriteService> _auditWriteService = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IOptions<AuthOptions>> _mockOptions = new();

    private GetAuthTokenFunction _sut;

    //TODO get passing
    [Fact]
    public async Task Invoke_GetAuthToken_WithValidToken_RecordsAuthLogin()
    {
        const string user = "test@test.com";

        ConfigureMocks();

        var actionResult = await _sut.RunAsync(CreateRequest());

        //TODO refactor in a more robust way
        await Task.Delay(100);

        actionResult.Should().BeEquivalentTo(new OkObjectResult(new { token = "" }));

        _auditWriteService.Verify(
            x => x.RecordAuth(It.IsAny<string>(), It.IsAny<DateTime>(), user, AuditAuthActionType.Login),
            Times.Once);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString("?from=2024-12-01&until=2024-12-08");
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }

    private void ConfigureMocks()
    {
        _sut = new GetAuthTokenFunction(_httpClientFactory.Object, _auditWriteService.Object, _mockOptions.Object);
    }
}
