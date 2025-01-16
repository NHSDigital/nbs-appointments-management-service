using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Nhs.Appointments.Api.Auth;

public static partial class AuthUtilities
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

    /// <summary>
    ///     Using first the email claim (if exists),
    ///     Then falling back to the subject (if it has regex of an email)
    /// </summary>
    public static string GetEmailFromToken(JwtSecurityToken token)
    {
        var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);

        if (emailClaim != null)
        {
            return emailClaim.Value;
        }

        if (IsValidEmail(token.Subject))
        {
            return token.Subject;
        }

        throw new InvalidOperationException("Email could not be found in JWT");
    }

    private static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
