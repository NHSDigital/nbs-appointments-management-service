using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Nhs.Appointments.Core.Caching;

namespace Nhs.Appointments.Api.Auth;

public class JwksRetriever(IHttpClientFactory httpClientFactory, ICacheService cacheService)
    : IJwksRetriever
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(60); // ToDo inject this as options
    
    public async Task<IEnumerable<SecurityKey>> GetKeys(string jwksEndpoint)
    {
        return await cacheService.GetCacheValue(
            jwksEndpoint, 
            new CacheOptions<IEnumerable<SecurityKey>>(
            async () => await GetSecurityKeys(jwksEndpoint), 
            _cacheDuration
            ));
    }

    private async Task<IEnumerable<SecurityKey>> GetSecurityKeys(string jwksEndpoint)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var keys = await client.GetJsonWebKeySetAsync(jwksEndpoint);
            return keys.KeySet.Keys.ToList().ConvertAll(ToSecurityKey);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Unable to retrieve jwks from configured endpoint", ex);
        }
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
