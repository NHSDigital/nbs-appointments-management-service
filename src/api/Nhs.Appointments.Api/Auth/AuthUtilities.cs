using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nhs.Appointments.Api.Auth;

public static class AuthUtilities
{
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        var inputBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        // Special "url-safe" base64 encode.
        return Convert.ToBase64String(inputBytes)
          .Replace('+', '-') // replace URL unsafe characters with safe ones
          .Replace('/', '_') // replace URL unsafe characters with safe ones
          .Replace("=", ""); // no padding
    }

    public static string GetEmailFromToken(JwtSecurityToken token)
    {
        var emailClaim = token.Claims.FirstOrDefault(cl => cl.Type == "Email Address");

        if (emailClaim != null)
        {
            return emailClaim.Value;
        }

        throw new InvalidOperationException($"Email claim (Email Address) could not be found in JWT");
    }
}
