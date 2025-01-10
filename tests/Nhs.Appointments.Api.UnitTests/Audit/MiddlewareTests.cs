using System.Collections.Immutable;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Audit.Functions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Tests.Audit;

public class MiddlewareTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly Mock<FunctionExecutionDelegate> _functionExecutionDelegate = new();
    private readonly Mock<IRequestInspector> _requestInspector = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly TestableAuditMiddleware _sut = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();

    [Fact]
    public async Task Invoke_DoesNotProcessRequest_WhenUserNotAuthorized()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test@test.com");
        var httpRequest = new TestHttpRequestData(_functionContext.Object);

        ConfigureMocks(httpRequest);

        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);

        var assemblyLocation = typeof(AuthenticateFunction).Assembly.Location;

        _functionContext.Setup(x => x.FunctionDefinition).Returns(new MockFunctionDefinition("AuthenticateFunction",
            "Nhs.Appointments.Api.Functions.AuthenticateFunction.Run", assemblyLocation));

        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(NoSiteRequestInspector))).Returns(new NoSiteRequestInspector());
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);

        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        
        _sut.AuditToWrite.Should().NotBeNull();
        _sut.AuditToWrite.UserId.Should().Be("test@test.com");
        _sut.AuditToWrite.ActionType.Should().Be("AuthenticateFunction");
        _sut.AuditToWrite.SiteId.Should().BeNull();
        
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

    private class TestHttpRequestData(FunctionContext functionContext) : HttpRequestData(functionContext)
    {
        private readonly HttpHeadersCollection _headers = new();

        public override Stream Body => throw new NotImplementedException();

        public override HttpHeadersCollection Headers => _headers;

        public override IReadOnlyCollection<IHttpCookie> Cookies => throw new NotImplementedException();

        public override Uri Url => throw new NotImplementedException();

        public override IEnumerable<ClaimsIdentity> Identities => throw new NotImplementedException();

        public override string Method => throw new NotImplementedException();

        public override HttpResponseData CreateResponse()
        {
            throw new NotImplementedException();
        }
    }
}
