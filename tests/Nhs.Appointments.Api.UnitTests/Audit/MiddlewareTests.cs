using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Audit;

public class MiddlewareTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly Mock<FunctionExecutionDelegate> _functionExecutionDelegate = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly TestableAuditMiddleware _sut = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();

    [Theory]
    [InlineData("test@test.com", typeof(AuthenticateFunction), "Run")]
    [InlineData("user@test.com", typeof(AuthenticateFunction), "Run")]
    public async Task Invoke_NoSiteRequestInspector_AuditFunction(string userId, Type functionType, string method)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal(userId);
        var httpRequest = new TestHttpRequestData(_functionContext.Object);

        ConfigureMocks(httpRequest);

        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(NoSiteRequestInspector))).Returns(new NoSiteRequestInspector());

        await Assert_AuditToWriteModel(userId, null, functionType, method, userPrincipal);
    }

    [Theory]
    [InlineData("test@test.com", "site-A", typeof(CancelBookingFunction), "RunAsync")]
    [InlineData("user@test.com", "site-B", typeof(CancelBookingFunction), "RunAsync")]
    public async Task Invoke_SiteFromQueryStringInspector_AuditFunction(string userId, string siteId, Type functionType,
        string method)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal(userId);
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.Query.Add("site", siteId);

        ConfigureMocks(httpRequest);

        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(SiteFromQueryStringInspector)))
            .Returns(new SiteFromQueryStringInspector());

        await Assert_AuditToWriteModel(userId, siteId, functionType, method, userPrincipal);
    }

    [Theory]
    [InlineData("test@test.com", typeof(ApplyAvailabilityTemplateFunction), "RunAsync")]
    [InlineData("user@test.com", typeof(SetAvailabilityFunction), "RunAsync")]
    public async Task Invoke_SiteFromBodyInspector_AuditFunction(string userId, Type functionType, string method)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal(userId);
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody("{\"site\": \"site-A\"}");

        ConfigureMocks(httpRequest);

        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(SiteFromBodyInspector))).Returns(new SiteFromBodyInspector());

        await Assert_AuditToWriteModel(userId, "site-A", functionType, method, userPrincipal);
    }

    [Theory]
    [InlineData("test@test.com", typeof(QueryAvailabilityFunction), "RunAsync")]
    [InlineData("user@test.com", typeof(NotifyBookingCancelledFunction), "NotifyBookingCancelledAsync")]
    [InlineData("test@test.com", typeof(MakeBookingFunction), "RunAsync")]
    [InlineData("test@test.com", typeof(GetUserRoleAssignmentsFunction), "RunAsync")]
    [InlineData("user@test.com", typeof(NotifyUserRolesChangedFunction), "NotifyUserRolesChangedAsync")]
    [InlineData("test@test.com", typeof(GetSiteMetaDataFunction), "RunAsync")]
    [InlineData("test@test.com", typeof(TriggerBookingRemindersFunction), "RunAsync")]
    [InlineData("test@test.com", typeof(AuthenticateCallbackFunction), "Run")]
    public async Task Invoke_NoRequiredAudit_DoesNot_AuditFunction(string userId, Type functionType, string method)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal(userId);
        var httpRequest = new TestHttpRequestData(_functionContext.Object);

        ConfigureMocks(httpRequest);

        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);

        await Assert_AuditToWriteModel_Null(functionType, method, userPrincipal);
    }

    private async Task Assert_AuditToWriteModel(string userId, string siteId, Type functionType, string method,
        ClaimsPrincipal userPrincipal)
    {
        var assemblyLocation = functionType.Assembly.Location;

        _functionContext.Setup(x => x.FunctionDefinition).Returns(
            new MockFunctionDefinition(
                functionType.Name,
                $"{functionType.FullName}.{method}",
                assemblyLocation));

        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);

        _sut.AuditToWrite.Should().NotBeNull();
        _sut.AuditToWrite.UserId.Should().Be(userId);
        _sut.AuditToWrite.ActionType.Should().Be(functionType.Name);
        _sut.AuditToWrite.SiteId.Should().Be(siteId);

        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }

    private async Task Assert_AuditToWriteModel_Null(Type functionType, string method, ClaimsPrincipal userPrincipal)
    {
        var assemblyLocation = functionType.Assembly.Location;

        _functionContext.Setup(x => x.FunctionDefinition).Returns(
            new MockFunctionDefinition(
                functionType.Name,
                $"{functionType.FullName}.{method}",
                assemblyLocation));

        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);

        _sut.AuditToWrite.Should().BeNull();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }

    private void ConfigureMocks(HttpRequestData httpRequest)
    {
        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider);

        var mockHttpRequestDataFeature = new Mock<IHttpRequestDataFeature>();
        mockHttpRequestDataFeature.Setup(x => x.GetHttpRequestDataAsync(It.IsAny<FunctionContext>()))
            .ReturnsAsync(httpRequest);

        var mockFeatures = new Mock<IInvocationFeatures>();
        mockFeatures.Setup(x => x.Get<IHttpRequestDataFeature>()).Returns(mockHttpRequestDataFeature.Object);

        _functionContext.Setup(x => x.Features).Returns(mockFeatures.Object);
        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
    }

    private class TestableAuditMiddleware : Middleware
    {
        public Appointments.Audit.Models.Audit AuditToWrite { get; private set; }

        protected override async Task RecordAudit(Appointments.Audit.Models.Audit auditToWrite)
        {
            AuditToWrite = auditToWrite;
        }
    }

    private class MockFunctionDefinition(string name, string entrypoint, string pathToAssembly) : FunctionDefinition
    {
        public override ImmutableArray<FunctionParameter> Parameters { get; }
        public override string PathToAssembly => pathToAssembly;
        public override string EntryPoint => entrypoint;
        public override string Id { get; }
        public override string Name => name;
        public override IImmutableDictionary<string, BindingMetadata> InputBindings { get; }
        public override IImmutableDictionary<string, BindingMetadata> OutputBindings { get; }
    }
}
