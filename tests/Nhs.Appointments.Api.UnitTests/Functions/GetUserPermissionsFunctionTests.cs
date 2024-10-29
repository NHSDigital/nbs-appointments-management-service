using System.Security.Claims;
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

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetUserPermissionsFunctionTests
{
    private readonly Mock<IValidator<SiteBasedResourceRequest>> _validator = new();
    private readonly Mock<IPermissionChecker> _permissionChecker = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<GetUserPermissionsFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly GetUserPermissionsFunction _sut;
    
    public GetUserPermissionsFunctionTests()
    {
        _sut = new GetUserPermissionsFunction(_permissionChecker.Object, _validator.Object, _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<SiteBasedResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_GetsPermissions_ForSignedInUser()
    {
        var testPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(testPrincipal);
        var userPermissions = new[] { "Permission-1", "Permission-2" };
        _permissionChecker.Setup(x => x.GetPermissionsAsync("test@test.com", "1")).ReturnsAsync(userPermissions);
        var expectedResult = new PermissionsResponse
        {
            Permissions = ["Permission-1", "Permission-2"]
        };

        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(200);
        var actualResponse = await ReadResponseAsync<PermissionsResponse>(result.Content);
        actualResponse.Should().BeEquivalentTo(expectedResult);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString($"?site=1");
        return request;
    }
    
    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }


}
