using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Nhs.Appointments.Api.Auth;
using System.Security.Claims;

namespace Nhs.Appointments.Api.Tests.Auth
{
    public class AuthenticationMiddlewareTests
    {        
        private readonly Mock<IFunctionTypeInfoFeature> _functionTypeInfoFeature = new();
        private readonly Mock<IServiceProvider> _serviceProvider = new();       
        private readonly Mock<IRequestAuthenticatorFactory> _requestAuthenticatorFactory = new();
        private readonly Mock<IRequestAuthenticator> _requestAuthenticator = new();
        private readonly Mock<FunctionContext> _functionContext = new();        
        private readonly UserContextProvider _userContextProvider = new();

        [Fact]
        public async Task Invoke_ReturnsUnauthorized_WhenHeadersNotPresentInRequest()
        {
            var httpRequest = new TestHttpRequestData(_functionContext.Object);
            ConfigureMocks(httpRequest);
                                    
            var sut = new TestableAuthenticationMiddleware(_requestAuthenticatorFactory.Object);
            await sut.Invoke(_functionContext.Object, sut.Authenticate);
            sut.Authenticated.Should().BeFalse();    
            _userContextProvider.UserPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Invoke_ReturnsUnauthorized_WhenHeaderFormatIsIncorrect()
        {
            var httpRequest = new TestHttpRequestData(_functionContext.Object);
            httpRequest.Headers.Add("Authorization", "garbage");
            ConfigureMocks(httpRequest);

            var sut = new TestableAuthenticationMiddleware(_requestAuthenticatorFactory.Object);
            await sut.Invoke(_functionContext.Object, sut.Authenticate);
            sut.Authenticated.Should().BeFalse();
            _userContextProvider.UserPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Invoke_ReturnsUnauthorized_WhenAuthorizationSchemeIsNotSupported()
        {
            var httpRequest = new TestHttpRequestData(_functionContext.Object);
            httpRequest.Headers.Add("Authorization", "unsupported token");
            ConfigureMocks(httpRequest);
            _requestAuthenticatorFactory.Setup(x => x.CreateAuthenticator("unsupported")).Throws<NotSupportedException>();

            var sut = new TestableAuthenticationMiddleware(_requestAuthenticatorFactory.Object);
            await sut.Invoke(_functionContext.Object, sut.Authenticate);
            sut.Authenticated.Should().BeFalse();
            _userContextProvider.UserPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Invoke_ReturnsUnauthorized_WhenAuthorizationFails()
        {
            var httpRequest = new TestHttpRequestData(_functionContext.Object);
            httpRequest.Headers.Add("Authorization", "unsupported token");
            ConfigureMocks(httpRequest);
            _requestAuthenticator.Setup(x => x.AuthenticateRequest("token", httpRequest)).ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity()));
            _requestAuthenticatorFactory.Setup(x => x.CreateAuthenticator("unsupported")).Returns(_requestAuthenticator.Object);

            var sut = new TestableAuthenticationMiddleware(_requestAuthenticatorFactory.Object);
            await sut.Invoke(_functionContext.Object, sut.Authenticate);
            sut.Authenticated.Should().BeFalse();
            _userContextProvider.UserPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Invoke_ReturnsAuthorized_WhenAuthorizationSucceeds()
        {
            var userPrincipal = CreateAuthenticatedPrincipal();
            var httpRequest = new TestHttpRequestData(_functionContext.Object);
            httpRequest.Headers.Add("Authorization", "unsupported token");
            ConfigureMocks(httpRequest);
            _requestAuthenticator.Setup(x => x.AuthenticateRequest("token", httpRequest)).ReturnsAsync(userPrincipal);
            _requestAuthenticatorFactory.Setup(x => x.CreateAuthenticator("unsupported")).Returns(_requestAuthenticator.Object);

            var sut = new TestableAuthenticationMiddleware(_requestAuthenticatorFactory.Object);
            await sut.Invoke(_functionContext.Object, sut.Authenticate);
            sut.Authenticated.Should().BeTrue();
            _userContextProvider.UserPrincipal.Should().Be(userPrincipal);
        }

        [Fact]
        public async Task Invoke_AllowsAnonymousAccess_OnMarkedMethods()
        {
            var httpRequest = new TestHttpRequestData(_functionContext.Object);            
            ConfigureMocks(httpRequest);
                        
            _functionTypeInfoFeature.Setup(x => x.RequiresAuthentication).Returns(false);

            var sut = new TestableAuthenticationMiddleware(_requestAuthenticatorFactory.Object);
            await sut.Invoke(_functionContext.Object, sut.Authenticate);
            sut.Authenticated.Should().BeTrue();
            _userContextProvider.UserPrincipal.Should().BeNull();
        }

        private ClaimsPrincipal CreateAuthenticatedPrincipal()
        {
            var claimsIdentity = new ClaimsIdentity("ApiKey");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "ApiUser"));
            return new ClaimsPrincipal(claimsIdentity);
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
    }

    public class TestableAuthenticationMiddleware : AuthenticationMiddleware
    {
        private bool _authenticated;

        public TestableAuthenticationMiddleware(IRequestAuthenticatorFactory requestAuthenticatorFactory) : base(requestAuthenticatorFactory)
        {
        }

        protected override void HandleUnauthorizedAccess(FunctionContext context)
        {
            _authenticated = false;
        }

        public Task Authenticate(FunctionContext context)
        {
            _authenticated = true;
            return Task.CompletedTask;
        }

        public bool Authenticated => _authenticated;
    }    
}
