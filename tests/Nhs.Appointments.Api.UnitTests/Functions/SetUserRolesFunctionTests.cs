using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class SetUserRolesFunctionTests
{
    private readonly SetUserRolesFunctionTestProxy _sut;
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IValidator<SetUserRolesRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<ILogger<SetUserRolesFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    public SetUserRolesFunctionTests()
    {
        _sut = new SetUserRolesFunctionTestProxy(_userService.Object, _validator.Object, _userContext.Object, _logger.Object, _metricsRecorder.Object);
    }

    [Fact]
    public async Task InvokesUserServiceWhenRolesChange()
    {
        const string User = "test@user.com";
        string[] roles = ["role1"];
        const string scope = "site:some-site";

        var request = new SetUserRolesRequest { User = User, Roles = roles, Scope = scope };

        _userService.Setup(s => s.UpdateUserRoleAssignmentsAsync(It.Is<string>(x => x == User), It.Is<string>(x => x == scope), It.Is<IEnumerable<RoleAssignment>>(x => x.Any(role => role.Role == roles[0])))).Returns(Task.FromResult(new UpdateUserRoleAssignmentsResult(true, null, null)));
        
        await _sut.Invoke(request);

        _userService.Verify();
    }

    internal class SetUserRolesFunctionTestProxy : SetUserRolesFunction
    {
        private ILogger<SetUserRolesFunction> _logger;
        public SetUserRolesFunctionTestProxy(IUserService userService, IValidator<SetUserRolesRequest> validator, IUserContextProvider userContextProvider, ILogger<SetUserRolesFunction> logger, IMetricsRecorder metricsRecorder) : base(userService, validator, userContextProvider, logger, metricsRecorder)
        {
            _logger = logger;
        }

        public async Task<ApiResult<EmptyResponse>> Invoke(SetUserRolesRequest request)
        {
            return await HandleRequest(request, _logger);
        }
    }
}
