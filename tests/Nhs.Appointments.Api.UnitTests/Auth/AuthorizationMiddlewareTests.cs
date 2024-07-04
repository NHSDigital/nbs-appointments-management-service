using System.Security.Claims;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class AuthorizationMiddlewareTests
{
    private readonly Mock<IFunctionTypeInfoFeature> _functionTypeInfoFeature = new();
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IPermissionChecker> _permissionChecker = new();
    private readonly Mock<FunctionExecutionDelegate> _functionExecutionDelegate = new();
    private readonly TestableAuthorizationMiddleware _sut;

    public AuthorizationMiddlewareTests()
    {
        _sut = new TestableAuthorizationMiddleware(_permissionChecker.Object);
    }
    
    [Fact]
    public async Task Invoke_DoesNotProcessRequest_WhenUserNotAuthorized()
    {
        var userPrincipal = CreateAuthenticatedPrincipal();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        var siteIdValue = new Dictionary<object, object> { {"siteId", "1"} };
        ConfigureMocks(httpRequest);
        
        _functionTypeInfoFeature.Setup(x => x.RequiredPermission).Returns("permission1");
        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
        _functionContext.Setup(x => x.Items).Returns(siteIdValue);
        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);
        _permissionChecker.Setup(x => x.HasPermissionAsync("test@test.com", "1", "permission1")).ReturnsAsync(false);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        _sut.IsAuthorized.Should().BeFalse();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Never);
    }
    
    [Fact]
    public async Task Invoke_ProcessesRequest_WhenUserIsAuthorized()
    {
        var userPrincipal = CreateAuthenticatedPrincipal();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        var siteIdValue = new Dictionary<object, object> { {"siteId", "1"} };
        ConfigureMocks(httpRequest);
        ConfigureMocks(httpRequest);
        
        _functionTypeInfoFeature.Setup(x => x.RequiredPermission).Returns("permission1");
        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
        _functionContext.Setup(x => x.Items).Returns(siteIdValue);
        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);
        _permissionChecker.Setup(x => x.HasPermissionAsync("test@test.com", "1", "permission1")).ReturnsAsync(true);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        _sut.IsAuthorized.Should().BeTrue();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData((string)null)]
    public async Task Invoke_ReturnsAuthorized_WhenRequiredPermissionIsNullOrEmpty(string requiredPermission)
    {
        var userPrincipal = CreateAuthenticatedPrincipal();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        ConfigureMocks(httpRequest);
        _functionTypeInfoFeature.Setup(x => x.RequiredPermission).Returns(requiredPermission);
        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider.Object);
        _userContextProvider.Setup(x => x.UserPrincipal).Returns(userPrincipal);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        _sut.IsAuthorized.Should().BeTrue();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    private void ConfigureMocks(HttpRequestData httpRequest)
    {
        _serviceProvider.Setup(x => x.GetService(typeof(IUserContextProvider))).Returns(_userContextProvider);

        var mockHttpRequestDataFeature = new Mock<IHttpRequestDataFeature>();
        mockHttpRequestDataFeature.Setup(x => x.GetHttpRequestDataAsync(It.IsAny<FunctionContext>())).ReturnsAsync(httpRequest);

        var mockFeatures = new Mock<IInvocationFeatures>();
        mockFeatures.Setup(x => x.Get<IHttpRequestDataFeature>()).Returns(mockHttpRequestDataFeature.Object);
        mockFeatures.Setup(x => x.Get<IFunctionTypeInfoFeature>()).Returns(_functionTypeInfoFeature.Object);
            
        _functionTypeInfoFeature.Setup(x => x.RequiresAuthentication).Returns(true);

        _functionContext.Setup(x => x.Features).Returns(mockFeatures.Object);
        _functionContext.Setup(x => x.InstanceServices).Returns(_serviceProvider.Object);
    }
    
    private static ClaimsPrincipal CreateAuthenticatedPrincipal()
    {
        var claimsIdentity = new ClaimsIdentity("TestUser");
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, "test@test.com"));
        return new ClaimsPrincipal(claimsIdentity);
    }

    private class TestableAuthorizationMiddleware(IPermissionChecker permissionChecker) : AuthorizationMiddleware(permissionChecker)
    {
        private bool _isAuthorized = true;
        
        protected override void HandleUnauthorizedAccess(FunctionContext context)
        {
            _isAuthorized = false;
        }

        public bool IsAuthorized => _isAuthorized;
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
