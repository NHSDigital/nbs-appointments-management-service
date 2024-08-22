using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Events;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Messaging;
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
    private readonly Mock<IMessageBus> _bus = new();
    public SetUserRolesFunctionTests()
    {
        _sut = new SetUserRolesFunctionTestProxy(_userService.Object, _validator.Object, _userContext.Object, _logger.Object, _bus.Object);
    }

    [Fact]
    public async Task RaisesEventWhenRolesChange()
    {
        const string User = "test@user.com";
        string[] roles = ["role1"];
        const string scope = "any";
        var request = new SetUserRolesRequest { User = User, Roles = roles, Scope = scope };
        
        _bus.Setup(b => b.Send(It.Is<UserRolesChanged>(e => e.User == User && Enumerable.SequenceEqual(e.Roles, roles)))).Verifiable();
        
        await _sut.Invoke(request);
        
        _bus.Verify();
    }

    internal class SetUserRolesFunctionTestProxy : SetUserRolesFunction
    {
        public SetUserRolesFunctionTestProxy(IUserService userService, IValidator<SetUserRolesRequest> validator, IUserContextProvider userContextProvider, ILogger<SetUserRolesFunction> logger, IMessageBus bus) : base(userService, validator, userContextProvider, logger, bus)
        {
        }

        public async Task Invoke(SetUserRolesRequest request)
        {
            await HandleRequest(request, logger);
        }
    }
}
