using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core;
using System.Globalization;

namespace Nhs.Appointments.Api.Tests.Auth
{
    public class SignedRequestAuthenticatorTests
    {
        private readonly SignedRequestAuthenticator _sut;
        private readonly Mock<TimeProvider> _timeProvider = new();
        private readonly Mock<IUserService> _userService = new();
        private readonly Mock<IOptions<SignedRequestAuthenticator.Options>> _options = new();
        private readonly Mock<IRequestSigner> _requestSigner = new();
        private readonly Mock<FunctionContext> _functionContext = new();

        public SignedRequestAuthenticatorTests()
        {
            _sut = new SignedRequestAuthenticator(_timeProvider.Object, _userService.Object, _requestSigner.Object, _options.Object);
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenClientIdHeaderIsNotPresent()
        {
            var request = SetupTestData();
            request.Headers.Remove("ClientId");
            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenClientIdHeaderIsEmpty()
        {
            var request = SetupTestData();            
            request.Headers.Remove("ClientId");
            request.Headers.Add("ClientId", "");
            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenMultipleClientIdHeaderValuesArePresent()
        {
            var request = SetupTestData();
            request.Headers.Remove("ClientId");
            request.Headers.Add("ClientId", new string[] { "test", "test" });
            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenApiUserKeyCannotBeRetrieved()
        {
            var request = SetupTestData();
            _userService.Setup(x => x.GetApiUserSigningKey("test")).ThrowsAsync(new InvalidOperationException());            

            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenApiUserKeyIsBlank()
        {
            var request = SetupTestData();
            _userService.Setup(x => x.GetApiUserSigningKey("test")).ReturnsAsync("");            

            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenRequestIsTooOld()
        {
            var request = SetupTestData(minsSinceRequest: 5);
                                    
            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenRequestSignatureDoesNotMatch()
        {
            var request = SetupTestData();
            
            var result = await _sut.AuthenticateRequest("invalid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenRequestTimestampIsMissing()
        {
            var request = SetupTestData();
            request.Headers.Remove("RequestTimestamp");

            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenRequestTimestampIsIncorrect()
        {
            var request = SetupTestData();
            request.Headers.Remove("RequestTimestamp");
            request.Headers.Add("RequestTimestamp", "not-a-date");

            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsUnauthorized_WhenMultipleRequestTimestampHeaderValuesArePresent()
        {
            var requestDateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            var request = SetupTestData();
            request.Headers.Remove("RequestTimestamp");
            request.Headers.Add("RequestTimestamp", new string[] { requestDateTime, requestDateTime });

            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateRequest_ReturnsAuthorized_WhenRequestIsValid ()
        {
            var request = SetupTestData();
            var result = await _sut.AuthenticateRequest("valid_sig", request);
            result.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeTrue();
        }        

        private HttpRequestData SetupTestData(int minsSinceRequest = 0)
        {
            _userService.Setup(x => x.GetApiUserSigningKey("test")).ReturnsAsync("api_key");
            _requestSigner.Setup(x => x.SignRequestAsync(It.IsAny<HttpRequestData>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("valid_sig");

            var requestTimeStamp = DateTime.UtcNow;
            var request = new TestHttpRequestData(_functionContext.Object);
            request.Headers.Add("ClientId", "test");
            request.Headers.Add("RequestTimestamp", requestTimeStamp.ToString("o", CultureInfo.InvariantCulture));

            _options.Setup(x => x.Value).Returns(new SignedRequestAuthenticator.Options { RequestTimeTolerance = TimeSpan.FromMinutes(3) });
            _timeProvider.Setup(x => x.GetUtcNow()).Returns(requestTimeStamp.AddMinutes(minsSinceRequest));

            return request;
        }
    }
}
