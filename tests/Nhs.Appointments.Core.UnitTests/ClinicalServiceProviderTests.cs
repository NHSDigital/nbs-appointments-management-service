using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.UnitTests;
public class ClinicalServiceProviderTests
{
    private readonly Mock<IClinicalServiceStore> _storeMock;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly ClinicalServiceProvider _sut;
    private readonly List<ClinicalServiceType> _sampleServices = new()
    {
        new ClinicalServiceType { Value = "COVID:5_11", ServiceType = "COVID-19", Url = "https://www.nhs.uk/bookcovid" },
        new ClinicalServiceType { Value = "FLU:18_64", ServiceType = "flu", Url = "https://www.nhs.uk/bookflu" }
    };
    private const string _cacheKey = "clinical-service";

    public ClinicalServiceProviderTests()
    {
        _storeMock = new Mock<IClinicalServiceStore>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _sut = new ClinicalServiceProvider(_storeMock.Object, _memoryCacheMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsClinicalServiceTypes()
    {
        // Arrange
        _storeMock.Setup(x => x.Get()).ReturnsAsync(_sampleServices);

        // Act
        var result = await _sut.Get();

        // Assert
        var resultArray = result.ToArray();

        Assert.NotNull(result);
        Assert.Equal(2, resultArray.Length);
        Assert.Equal(_sampleServices[0].Value, resultArray[0].Value);
        Assert.Equal(_sampleServices[0].ServiceType, resultArray[0].ServiceType);
        Assert.Equal(_sampleServices[0].Url, resultArray[0].Url);
        Assert.Equal(_sampleServices[1].Value, resultArray[1].Value);
        Assert.Equal(_sampleServices[1].ServiceType, resultArray[1].ServiceType);
        Assert.Equal(_sampleServices[1].Url, resultArray[1].Url);
    }

    [Fact]
    public async Task Get_ReturnsClinicalServiceType()
    {
        // Arrange
        var service = "COVID:5_11";

        _storeMock.Setup(x => x.Get()).ReturnsAsync(_sampleServices);

        // Act
        var result = await _sut.Get(service);

        // Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () => Assert.Equal(_sampleServices[0].Value, result.Value),
            () => Assert.Equal(_sampleServices[0].ServiceType, result.ServiceType),
            () => Assert.Equal(_sampleServices[0].Url, result.Url)
        );
    }


    [Fact]
    public async Task GetFromCache_ReturnsFromCache_WhenPresent()
    {
        // Arrange
        object cached = _sampleServices;
        _memoryCacheMock.Setup(mc => mc.TryGetValue(_cacheKey, out cached)).Returns(true);

        // Act
        var result = await _sut.GetFromCache();

        // Assert
        Assert.Equal(_sampleServices, result);
        _storeMock.Verify(s => s.Get(), Times.Never);
    }

    [Fact]
    public async Task GetFromCache_ReturnsFromStore_WhenCacheIsEmpty()
    {
        // Arrange
        object cached;
        _memoryCacheMock.Setup(mc => mc.TryGetValue(_cacheKey, out cached)).Returns(false);
        _storeMock.Setup(s => s.Get()).ReturnsAsync(_sampleServices);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await _sut.GetFromCache();

        // Assert
        Assert.Equal(_sampleServices, result);
        _storeMock.Verify(s => s.Get(), Times.Once);
    }
}
