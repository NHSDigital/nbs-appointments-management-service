using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public class JwksRetriever : IJwksRetriever
{
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpClientFactory _httpClientFactory;

    public JwksRetriever(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<SecurityKey>> GetKeys(string jwksEndpoint)
    {
        var client = _httpClientFactory.CreateClient();
        
        var result = _memoryCache.Get<IEnumerable<SecurityKey>>(jwksEndpoint);
        if (result == null)
        {
            try
            {
                var jwksResponse = await client.GetJsonWebKeySetAsync(jwksEndpoint);
                result = jwksResponse.KeySet.Keys.ToList().ConvertAll(ToSecurityKey);
                _memoryCache.Set(jwksEndpoint, result, DateTimeOffset.UtcNow.AddHours(1));
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Unable to retrieve jwks from configured endpoint", ex);
            }
        }
        return result;                
    }

    private static SecurityKey ToSecurityKey(IdentityModel.Jwk.JsonWebKey webKey)
    {
        var e = Base64Url.Decode(webKey.E);
        var n = Base64Url.Decode(webKey.N);

        return new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
        {
            KeyId = webKey.Kid
        };
    }
}
