using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth
{
    public class SignedRequestAuthenticator(TimeProvider dateTimeProvider, IUserService userService, IRequestSigner requestSigner, IOptions<SignedRequestAuthenticator.Options> options) : IRequestAuthenticator
    {        
        public async Task<ClaimsPrincipal> AuthenticateRequest(string authenticationToken, HttpRequestData requestData)
        {
            if (requestData.Headers.Contains("ClientId") == false || requestData.Headers.Contains("RequestTimestamp") == false)
            {
                return Unauthorized();
            }

            var clientId = requestData.Headers.GetValues("ClientId").SingleOrDefault();

            if (string.IsNullOrEmpty(clientId))
            {
                return Unauthorized();
            }

            var getSigningKeyOp = await TryPattern.TryAsync(() => userService.GetApiUserSigningKey(clientId));
            if (getSigningKeyOp.Completed == false || string.IsNullOrEmpty(getSigningKeyOp.Result))
            {
                return Unauthorized();
            }                       

            try
            {
                var requestTimestamp = requestData.Headers.GetValues("RequestTimestamp").SingleOrDefault();
                var requestDateTime = DateTime.Parse(requestTimestamp, null, DateTimeStyles.RoundtripKind);
                var requestAge = dateTimeProvider.GetUtcNow().Subtract(requestDateTime);

                if (requestAge < options.Value.RequestTimeTolerance)
                {
                    var expectedSignature = await requestSigner.SignRequestAsync(requestData, requestTimestamp, getSigningKeyOp.Result);

                    if (expectedSignature == authenticationToken)
                    {
                        var claimsIdentity = new ClaimsIdentity("ApiKey");
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, $"api@{clientId}"));
                        return new ClaimsPrincipal(claimsIdentity);
                    }
                }
            }
            catch(FormatException)
            {

            }

            return Unauthorized();
        }

        private ClaimsPrincipal Unauthorized()
        {
            ClaimsIdentity unauthenticated = new ClaimsIdentity();
            return new ClaimsPrincipal(unauthenticated);
        }

        public class Options
        {            
            public TimeSpan RequestTimeTolerance { get; set; }
        }
    }
}
