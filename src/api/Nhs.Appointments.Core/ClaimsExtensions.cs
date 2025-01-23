using System.Security.Claims;

namespace Nhs.Appointments.Core
{
    public static class ClaimsExtensions
    {
        public static string GetUserEmail(this IEnumerable<Claim> claims) 
        {
            var claimsToCheck = new[] { ClaimTypes.Email, "Email Address", ClaimTypes.NameIdentifier, "sub" };
            foreach (var claim in claimsToCheck)
            {
                var emailClaim = claims.SingleOrDefault(x => x.Type == claim);
                if (emailClaim != null)
                    return emailClaim.Value;
            }
                            
            return string.Empty;
        }            
    }
}
