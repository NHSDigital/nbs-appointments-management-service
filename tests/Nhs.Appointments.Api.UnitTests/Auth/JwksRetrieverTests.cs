using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class JwksRetrieverTests
{
    private readonly JwksRetriever _sut;
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IMemoryCache> _memoryCache = new();

    public JwksRetrieverTests()
    {
        _sut = new JwksRetriever(_httpClientFactory.Object, _memoryCache.Object);
    }

    [Fact]
    public async Task GetKeys_ReturnsKeysFromCache_WhenTheyArePresent()
    {
        object dummyKeys = new List<SecurityKey> { new DummySecurityKey() };
        var jwksEndpoint = "http://test.oauth.com/jwks";
        _memoryCache.Setup(x => x.TryGetValue(jwksEndpoint, out dummyKeys)).Returns(true);
        var keys = await _sut.GetKeys(jwksEndpoint);

        keys.Should().HaveCount(1);
        keys.First().Should().BeOfType<DummySecurityKey>();
    }

    [Fact]
    public async Task GetKeys_RetrievesKeysFromWellknownEndpoint_WhenTheyAreNotCached()
    {
        var mockEntry = new Mock<ICacheEntry>();
        _memoryCache.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(mockEntry.Object);
        var mockHttpClient = new MockHttpClient();
        _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Client);
        
        mockHttpClient.EnqueueStringResponse("{\"keys\":[{\"kty\":\"RSA\",\"use\":\"sig\",\"kid\":\"AB422DE9273F9B73151F7DF58B2B542B\",\"e\":\"AQAB\",\"n\":\"qH1NX1d4k09nyIwGEnzbZq12BLpif-tGltLlsbOfr9MUvSqpRrRdZWC-ya7Dw936h3OFC8uCVDDsoAi6BEdPFRQhgPDMQOWGcOn_kyiL4_EbXFQlmZcoxszUOsx8vD3F-Ve4pOI8GKEOp8T81EcvgY6wM0S-yt3HoTnMRCJwnbpI4FJ-0_auweW0d9TqOPH4Wx_ZuL-zglzOaekbeFsSugX5iGJJz1gyDGpX3IwqguuaJxg5dop9nz-EcmvMdQyVLrHPklATqGQUQC5bYn3ADx-QrwOZ4dcnp2wlRK2ErF3POHVPPysYKDEqaCddb8cuzp9IZU6fgxFaqhAvH_hUkQ\",\"alg\":\"RS256\"}]}");

        var jwksEndpoint = "http://test.oauth.com/jwks";            
        var keys = await _sut.GetKeys(jwksEndpoint);

        keys.Should().HaveCount(1);            
    }
}

public class DummySecurityKey : SecurityKey
{
    public override int KeySize => throw new NotImplementedException();
}
