using System.Net;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class SetUserRolesFunctionTests
{
    private readonly Mock<ILogger<SetUserRolesFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly SetUserRolesFunctionTestProxy _sut;
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IValidator<SetUserRolesRequest>> _validator = new();

    public SetUserRolesFunctionTests()
    {
        _sut = new SetUserRolesFunctionTestProxy(_userService.Object, _validator.Object, _userContext.Object,
            _logger.Object, _metricsRecorder.Object);
    }

    [Fact]
    public async Task InvokesUserServiceWhenRolesChange()
    {
        const string User = "test@user.com";
        string[] roles = ["role1"];
        const string scope = "site:some-site";
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user2@testdomain.com");

        var request = new SetUserRolesRequest { User = User, Roles = roles, Scope = scope };

        _userService.Setup(s => s.UpdateUserRoleAssignmentsAsync(
                It.Is<string>(x => x == User), It.Is<string>(x => x == scope),
                It.Is<IEnumerable<RoleAssignment>>(x => x.Any(role => role.Role == roles[0]))))
            .Returns(Task.FromResult(new UpdateUserRoleAssignmentsResult(true, null, null)));
        _userContext.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);

        await _sut.Invoke(request);

        _userService.Verify();
    }

    [Fact]
    public async Task ReturnsBadRequestResponse_WhenUserTriesToUpdateRolesForThemself()
    {
        const string User = "test@user.com";
        string[] roles = ["role1"];
        const string scope = "site:some-site";
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test@user.com");
        const string expectedReason = "You cannot update the role assignments of the currently logged in user.";

        var request = new SetUserRolesRequest { User = User, Roles = roles, Scope = scope };

        _userService.Setup(s => s.UpdateUserRoleAssignmentsAsync(
                It.Is<string>(x => x == User), It.Is<string>(x => x == scope),
                It.Is<IEnumerable<RoleAssignment>>(x => x.Any(role => role.Role == roles[0]))))
            .Returns(Task.FromResult(new UpdateUserRoleAssignmentsResult(true, null, null)));
        _userContext.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);

        var result = await _sut.Invoke(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Reason.Should().Be(expectedReason);
    }

    internal class SetUserRolesFunctionTestProxy : SetUserRolesFunction
    {
        private readonly ILogger<SetUserRolesFunction> _logger;

        public SetUserRolesFunctionTestProxy(
            IUserService userService,
            IValidator<SetUserRolesRequest> validator,
            IUserContextProvider userContextProvider,
            ILogger<SetUserRolesFunction> logger,
            IMetricsRecorder metricsRecorder)
            : base(userService, validator, userContextProvider, logger, metricsRecorder)
        {
            _logger = logger;
        }

        public async Task<ApiResult<EmptyResponse>> Invoke(SetUserRolesRequest request)
        {
            return await HandleRequest(request, _logger);
        }
    }
}
