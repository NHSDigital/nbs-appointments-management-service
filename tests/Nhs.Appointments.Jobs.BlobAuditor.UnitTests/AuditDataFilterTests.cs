using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.BlobAuditor.UnitTests;

public class AuditDataFilterTests
{
    private readonly Mock<IOptions<DataFilterOptions>> _options = new();
    private readonly Mock<ILogger<AuditDataFilter>> _logger = new();

    private readonly IDataFilter<JObject> _sut;
    
    public AuditDataFilterTests()
    {
        _options.Setup(x => x.Value).Returns(new DataFilterOptions
        {
            Sites = [
                "5252d964-3ab2-4bf6-8059-c61620de38c6",
                "ade9c5cd-4f57-4b10-9b8f-26e9a6cfeeee",
                "645b1032-9c40-4c51-bb57-971f277cd9da",
                "354e189a-0293-4ad8-aaeb-3be205109a5a"
            ]
        });
        
        _sut = new AuditDataFilter(_options.Object, _logger.Object);
    }

    [Fact]
    public void IsValidItem_AcceptsValidData()
    {
        var validItems = new List<JObject>
        {
            new()
            {
                { "docType", "booking" },
                { "site", "ade9c5cd-4f57-4b10-9b8f-26e9a6cfeeee" },
                { "from", new DateTime(2025, 10, 1) },
                { "status", "Booked" }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "645b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "patient" },
                { "id", "123" },
            }
        };

        foreach (var item in validItems)
        {
            Assert.True(_sut.IsValidItem(item));
        }
    }
    
    [Fact]
    public void IsValidItem_RejectsOtherSites()
    {
        var validItems = new List<JObject>
        {
            new()
            {
                { "docType", "booking" },
                { "site", "fde9c5cd-4f57-4b10-9b8f-26e9a6cfeeee" },
                { "from", new DateTime(2025, 10, 1) },
                { "status", "Booked" }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "145b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            },
        };

        foreach (var item in validItems)
        {
            Assert.False(_sut.IsValidItem(item));
        }
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()!.Contains("Site 'fde9c5cd-4f57-4b10-9b8f-26e9a6cfeeee' is filtered out.")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!
            ), Times.Once
        );
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()!.Contains("Site '145b1032-9c40-4c51-bb57-971f277cd9da' is filtered out.")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!
            ), Times.Once
        );
    }
    
    [Fact]
    public void IsValidItem_DoesNot_RejectOtherDocumentTypes()
    {
        var validItems = new List<JObject>
        {
            new()
            {
                { "docType", "availability_created_event" },
                { "site", "ade9c5cd-4f57-4b10-9b8f-26e9a6cfeeee" },
                { "from", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "booking_other" },
                { "site", "645b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            }
        };

        foreach (var item in validItems)
        {
            Assert.True(_sut.IsValidItem(item));
        }
    }
    
    [Fact]
    public void IsValidItem_AuditsProvisionalBookingData()
    {
        var validItems = new List<JObject>
        {
            new()
            {
                { "docType", "booking" },
                { "site", "ade9c5cd-4f57-4b10-9b8f-26e9a6cfeeee" },
                { "from", new DateTime(2025, 10, 1) },
                { "status", "Provisional" }
            },
            new()
            {
                { "docType", "booking" },
                { "site", "354e189a-0293-4ad8-aaeb-3be205109a5a" },
                { "from", new DateTime(2025, 10, 2) },
                { "status", "Provisional" }
            },
        };

        foreach (var item in validItems)
        {
            Assert.True(_sut.IsValidItem(item));
        }
    }
    
    [Fact]
    public void NullSites_IsValidItem_AcceptsAllSiteData()
    {
        _options.Setup(x => x.Value).Returns(new DataFilterOptions());
        
        var sut = new AuditDataFilter(_options.Object, _logger.Object);
        
        var validItems = new List<JObject>
        {
            new()
            {
                { "docType", "booking" },
                { "site", "dsfrgsdfgd" },
                { "from", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "645b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "789b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "f" },
                { "date", new DateTime(2025, 10, 1) }
            }
        };

        foreach (var item in validItems)
        {
            Assert.True(sut.IsValidItem(item));
        }
    }
    
    [Fact]
    public void EmptySites_IsValidItem_AcceptsAllSiteData()
    {
        _options.Setup(x => x.Value).Returns(new DataFilterOptions
        {
            Sites = []
        });
        
        var sut = new AuditDataFilter(_options.Object, _logger.Object);
        
        var validItems = new List<JObject>
        {
            new()
            {
                { "docType", "booking" },
                { "site", "dsfrgsdfgd" },
                { "from", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "645b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "789b1032-9c40-4c51-bb57-971f277cd9da" },
                { "date", new DateTime(2025, 10, 1) }
            },
            new()
            {
                { "docType", "daily_availability" },
                { "site", "f" },
                { "date", new DateTime(2025, 10, 1) }
            }
        };

        foreach (var item in validItems)
        {
            Assert.True(sut.IsValidItem(item));
        }
    }
}
