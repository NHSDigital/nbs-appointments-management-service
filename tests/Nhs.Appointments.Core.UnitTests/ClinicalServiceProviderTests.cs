using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.UnitTests;
public class ClinicalServiceProviderTests
{
    private readonly Mock<IClinicalServiceStore> _storeMock;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly ClinicalServiceProvider _sut;
    private readonly List<ClinicalServiceType> _sampleServices = new()
    {
        new ClinicalServiceType { Value = "COVID:5_11", Vaccine = "COVID-19", Url = "https://www.nhs.uk/bookcovid" },
        new ClinicalServiceType { Value = "FLU:18_64", Vaccine = "flu", Url = "https://www.nhs.uk/bookflu" }
    };

    public ClinicalServiceProviderTests()
    {
        _storeMock = new Mock<IClinicalServiceStore>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _sut = new ClinicalServiceProvider(_storeMock.Object, _memoryCacheMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsFromCache_WhenPresent()
    {
        // Arrange
        object cached = _sampleServices;
        _memoryCacheMock.Setup(mc => mc.TryGetValue("clinical-service", out cached)).Returns(true);

        // Act
        var result = await _sut.Get();

        // Assert
        Assert.Equal(_sampleServices, result);
        _storeMock.Verify(s => s.Get(), Times.Never);
    }

    [Fact]
    public async Task Get_ReturnsFromStore_WhenCacheIsEmpty()
    {
        // Arrange
        object cached;
        _memoryCacheMock.Setup(mc => mc.TryGetValue("clinical-service", out cached)).Returns(false);
        _storeMock.Setup(s => s.Get()).ReturnsAsync(_sampleServices);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await _sut.Get();

        // Assert
        Assert.Equal(_sampleServices, result);
        _storeMock.Verify(s => s.Get(), Times.Once);
    }

    [Fact]
    public async Task GetVaccineType_ReturnsCorrectValue()
    {
        // Arrange
        object cached = _sampleServices;
        _memoryCacheMock.Setup(mc => mc.TryGetValue("clinical-service", out cached)).Returns(true);

        // Act
        var result = await _sut.GetVaccineType("COVID:5_11");

        // Assert
        Assert.Equal("COVID-19", result);
    }

    [Fact]
    public async Task GetServiceUrl_ReturnsCorrectUrl()
    {
        // Arrange
        object cached = _sampleServices;
        _memoryCacheMock.Setup(mc => mc.TryGetValue("clinical-service", out cached)).Returns(true);

        // Act
        var result = await _sut.GetServiceUrl("FLU:18_64");

        // Assert
        Assert.Equal("https://www.nhs.uk/bookflu", result);
    }

    [Fact]
    public async Task GetVaccineType_ReturnsNull_WhenServiceNotFound()
    {
        // Arrange
        object cached = _sampleServices;
        _memoryCacheMock.Setup(mc => mc.TryGetValue("clinical-service", out cached)).Returns(true);

        // Act
        var result = await _sut.GetVaccineType("nonexistent");

        // Assert
        Assert.Null(result);
    }
}
