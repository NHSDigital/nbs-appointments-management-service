// See https://aka.ms/new-console-template for more information

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;


var rsa = RSA.Create(2048);

var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
{
    CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
};

var encodedPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

Console.WriteLine("PUBLIC KEY FOR CONFIG:");
Console.WriteLine(encodedPublicKey);

Console.WriteLine();

// loop around different users
var claims = new List<Claim>
{
    new(ClaimTypes.Name, "user1@user.com"),
};

var token = new JwtSecurityToken(
    claims: claims,
    issuer: "local",
    audience: "local",
    notBefore: DateTime.UtcNow.AddDays(-1),
    expires: DateTime.UtcNow.AddYears(10),
    signingCredentials: signingCredentials
);
var jwt = new JwtSecurityTokenHandler().WriteToken(token);

Console.WriteLine("Token:");
Console.WriteLine(jwt);

