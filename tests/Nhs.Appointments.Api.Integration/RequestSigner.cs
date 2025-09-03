using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nhs.Appointments.Api.Integration;

public class RequestSigner(TimeProvider time, string signingKey) : IRequestSigner
{
    public async Task SignRequestAsync(HttpRequestMessage request)
    {           
        var method = request.Method.ToString();
        var path = request.RequestUri!.AbsolutePath;
        var queryString = request.RequestUri.Query;
        var content = "";
        if (request.Content != null)
            content = await request.Content.ReadAsStringAsync();
        content += DecodeQueryString(queryString);

        using var hash = SHA256.Create();
        var hashedContentBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(content));
        var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);

        var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(signingKey) };
        var requestTimestamp = time.GetUtcNow().ToString("o", CultureInfo.InvariantCulture);
        var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
        var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));

        request.Headers.Authorization = new("Signed", Convert.ToBase64String(sigBytes));
        request.Headers.Add("RequestTimestamp", requestTimestamp);
    }

    private static string DecodeQueryString(string queryString)
    {
        return queryString.Contains('?') ? HttpUtility.UrlDecode(queryString.Substring(1)) : string.Empty;
    }
}

public interface IRequestSigner
{
    Task SignRequestAsync(HttpRequestMessage request);
}
