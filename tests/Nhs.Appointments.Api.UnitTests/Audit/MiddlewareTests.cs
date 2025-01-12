using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Audit.Services;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Audit;

public class MiddlewareTests
{
    private readonly Mock<IAuditWriteService> _auditWriteService = new();
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly Mock<FunctionExecutionDelegate> _functionExecutionDelegate = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private Middleware _sut;

    [Theory]
    [InlineData("test@test.com", typeof(AuthenticateFunction), "Run")]
    [InlineData("user@test.com", typeof(AuthenticateFunction), "Run")]
    public async Task Invoke_NoSiteRequestInspector_RecordFunction(string userId, Type functionType, string method)
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);

        ConfigureMocks(httpRequest, userId, functionType, method);

        _serviceProvider.Setup(x => x.GetService(typeof(NoSiteRequestInspector))).Returns(new NoSiteRequestInspector());

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);

        //TODO refactor in a more robust way
        await Task.Delay(100);

        // Assert
        //TODO add execution time middleware to verify timestamp logged?
        _auditWriteService.Verify(s => s.RecordFunction(It.IsAny<DateTime>(), userId, functionType.Name, null),
            Times.Once);
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }

    [Theory]
    [InlineData("test@test.com", "site-A", typeof(CancelBookingFunction), "RunAsync")]
    [InlineData("user@test.com", "site-B", typeof(CancelBookingFunction), "RunAsync")]
    public async Task Invoke_SiteFromQueryStringInspector_RecordFunction(string userId, string siteId, Type functionType,
        string method)
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.Query.Add("site", siteId);

        ConfigureMocks(httpRequest, userId, functionType, method);

        _serviceProvider.Setup(x => x.GetService(typeof(SiteFromQueryStringInspector)))
            .Returns(new SiteFromQueryStringInspector());

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);

        //TODO refactor in a more robust way
        await Task.Delay(100);

        _auditWriteService.Verify(x => x.RecordFunction(It.IsAny<DateTime>(), userId, functionType.Name, siteId),
            Times.Once);
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }

    [Theory]
    [InlineData("test@test.com", typeof(ApplyAvailabilityTemplateFunction), "RunAsync")]
    [InlineData("user@test.com", typeof(SetAvailabilityFunction), "RunAsync")]
    public async Task Invoke_SiteFromBodyInspector_RecordFunction(string userId, Type functionType, string method)
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody("{\"site\": \"site-A\"}");

        ConfigureMocks(httpRequest, userId, functionType, method);

        _serviceProvider.Setup(x => x.GetService(typeof(SiteFromBodyInspector))).Returns(new SiteFromBodyInspector());

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);

        //TODO refactor in a more robust way
        await Task.Delay(100);

        _auditWriteService.Verify(x => x.RecordFunction(It.IsAny<DateTime>(), userId, functionType.Name, "site-A"),
            Times.Once);
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
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
    public async Task Invoke_NoRequiredAuditAttribute_DoesNot_RecordFunction(string userId, Type functionType, string method)
    {
        ConfigureMocks(new TestHttpRequestData(_functionContext.Object), userId, functionType, method);

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);

        //TODO refactor in a more robust way
        await Task.Delay(100);

        // Assert
        _auditWriteService.Verify(
            s => s.RecordFunction(It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }

    private void ConfigureMocks(HttpRequestData httpRequestData, string userId, Type functionType, string method)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal(userId);

        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider);

        var mockHttpRequestDataFeature = new Mock<IHttpRequestDataFeature>();
        mockHttpRequestDataFeature.Setup(x => x.GetHttpRequestDataAsync(It.IsAny<FunctionContext>()))
            .ReturnsAsync(httpRequestData);

        var mockFeatures = new Mock<IInvocationFeatures>();
        mockFeatures.Setup(x => x.Get<IHttpRequestDataFeature>()).Returns(mockHttpRequestDataFeature.Object);

        _functionContext.Setup(x => x.Features).Returns(mockFeatures.Object);
        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);

        var assemblyLocation = functionType.Assembly.Location;

        _functionContext.Setup(x => x.FunctionDefinition).Returns(
            new MockFunctionDefinition(
                functionType.Name,
                $"{functionType.FullName}.{method}",
                assemblyLocation));

        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);

        _sut = new Middleware(_auditWriteService.Object);
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
