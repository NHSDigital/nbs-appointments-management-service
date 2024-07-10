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
    public class SignedRequestAuthenticator(TimeProvider dateTimeProvider, IApiClientService apiClients, IOptions<SignedRequestAuthenticator.Options> options) : IRequestAuthenticator
    {        
        public async Task<ClaimsPrincipal> AuthenticateRequest(string authenticationToken, HttpRequestData requestData)
        {
            try
            {
                var clientId = requestData.Headers.GetValues("ClientId").SingleOrDefault();

                if(string.IsNullOrEmpty(clientId))
                {
                    return Unauthorized();
                }

                var clientProfile = await apiClients.Get(clientId);

                if(clientProfile == null)
                {
                    return Unauthorized();
                }

                var requestTimestamp = requestData.Headers.GetValues("RequestTimestamp").SingleOrDefault();
                var requestDateTime = DateTime.Parse(requestTimestamp, null, DateTimeStyles.RoundtripKind);
                var requestAge = dateTimeProvider.GetUtcNow().Subtract(requestDateTime);

                if (requestAge < options.Value.RequestTimeTolerance)
                {
                    var expectedSignature = await RequestSigner.SignRequestAsync(requestData, requestTimestamp, clientProfile.SigningKey);

                    if (expectedSignature == authenticationToken)
                    {
                        var claimsIdentity = new ClaimsIdentity("ApiKey");
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, clientId));
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
