using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth
{
    public class SignedRequestAuthenticator(TimeProvider dateTimeProvider, IOptions<SignedRequestAuthenticator.Options> options) : IRequestAuthenticator
    {        
        public async Task<ClaimsPrincipal> AuthenticateRequest(string authenticationToken, HttpRequestData requestData)
        {
            try
            {
                var requestTimestamp = requestData.Headers.GetValues("RequestTimestamp").SingleOrDefault();
                var requestDateTime = DateTime.Parse(requestTimestamp, null, DateTimeStyles.RoundtripKind);
                var requestAge = dateTimeProvider.GetUtcNow().Subtract(requestDateTime);

                if (requestAge < options.Value.RequestTimeTolerance)
                {
                    var expectedSignature = await RequestSigner.SignRequestAsync(requestData, requestTimestamp, options.Value.SigningKey);

                    if (expectedSignature == authenticationToken)
                    {
                        var claimsIdentity = new ClaimsIdentity("ApiKey");
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "ApiUser"));
                        return new ClaimsPrincipal(claimsIdentity);
                    }
                }
            }
            catch(FormatException)
            {

            }

            ClaimsIdentity unauthenticated = new ClaimsIdentity();
            return new ClaimsPrincipal(unauthenticated);
        }

        public class Options
        {
            public string SigningKey { get; set; }
            public TimeSpan RequestTimeTolerance { get; set; }
        }
    }
}
