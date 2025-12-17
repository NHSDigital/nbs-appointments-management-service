using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;
using Nhs.Appointments.Jobs.BlobAuditor.Sink.Config;

namespace Nhs.Appointments.Jobs.BlobAuditor.UnitTests.Sink;

public class ItemExclusionProcessorTests
{
    private readonly Mock<IOptions<List<SinkExclusion>>> _options = new();
    private readonly ItemExclusionProcessor _processor;

    public ItemExclusionProcessorTests()
    {
        _processor = new ItemExclusionProcessor(_options.Object);
    }

    [Fact]
    public void Apply_NoExclusions_ItemReturned()
    {
        // Arrange
        var item = new JObject
        {
            ["id"] = "123",
            ["secret"] = "hidden"
        };

        _options.Setup(o => o.Value).Returns(() => null);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        Assert.True(JToken.DeepEquals(item, result));
    }

    [Fact]
    public void Apply_NoExcludedPaths_ItemReturned()
    {
        // Arrange
        var config = new List<SinkExclusion>
        {
            new() { Source = "booking_data", ExcludedPaths = new List<string>() }
        };
        var item = new JObject
        {
            ["id"] = "123",
            ["secret"] = "hidden"
        };

        _options.Setup(o => o.Value).Returns(config);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        Assert.True(JToken.DeepEquals(item, result));
    }

    [Fact]
    public void Apply_NotValidExclusionPath_ItemReturned()
    {
        // Arrange
        var config = new List<SinkExclusion>
        {
            new() { Source = "booking_data", ExcludedPaths = new List<string>(){ "secrett" } }
        };
        var item = new JObject
        {
            ["id"] = "123",
            ["secret"] = "hidden"
        };

        _options.Setup(o => o.Value).Returns(config);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        Assert.True(JToken.DeepEquals(item, result));
    }

    [Fact]
    public void Apply_PropertyExclusionPattern_PropertyRemoved()
    {
        // Arrange
        var config = new List<SinkExclusion>
        {
            new() { Source = "booking_data", ExcludedPaths = new List<string>(){ "secret" } }
        };
        var item = new JObject
        {
            ["id"] = "123",
            ["secret"] = "hidden"
        };

        _options.Setup(o => o.Value).Returns(config);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        Assert.False(result.ContainsKey("secret"));
    }

    [Fact]
    public void Apply_NestedPropertyExclusionPattern_NestedPropertyRemoved()
    {
        // Arrange
        var config = new List<SinkExclusion>
        {
            new() { Source = "booking_data", ExcludedPaths = new List<string>(){ "contactDetails.phone" } }
        };
        var item = new JObject
        {
            ["id"] = "123",
            ["contactDetails"] = new JObject
            {
                ["phone"] = "123456",
                ["email"] = "test@mail.com"
            }
        };

        _options.Setup(o => o.Value).Returns(config);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        var contactDetails = (JObject)result["contactDetails"];
        Assert.False(contactDetails.ContainsKey("phone"));
        Assert.True(contactDetails.ContainsKey("email"));
    }

    [Fact]
    public void Apply_ArrayIndexExclusionPattern_ArrayElementRemoved()
    {
        // Arrange
        var config = new List<SinkExclusion>
        {
            new() { Source = "booking_data", ExcludedPaths = new List<string>(){ "contactDetails[0]" } }
        };
        var item = new JObject
        {
            ["id"] = "123",
            ["contactDetails"] = new JArray
            {
                "test@mail.com",
                "123456"
            }
        };

        _options.Setup(o => o.Value).Returns(config);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        var contactDetails = (JArray)result["contactDetails"];

        Assert.Single(contactDetails);
        Assert.DoesNotContain("test@mail.com", contactDetails.Select(c => c.Value<string>()));
        Assert.Contains("123456", contactDetails.Select(c => c.Value<string>()));
    }

    [Fact]
    public void Apply_ArrayPropertyExclusionPattern_ArrayPropertyRemovedFromAllElements()
    {
        // Arrange
        var config = new List<SinkExclusion>
        {
            new() { Source = "booking_data", ExcludedPaths = new List<string>(){ "contactDetails[*].email" } }
        };
        var item = new JObject
        {
            ["id"] = "123",
            ["contactDetails"] = new JArray
            {
                new JObject
                {
                    ["email"] = "john@example.com",
                    ["phone"] = "1234567890"
                },
                new JObject
                {
                    ["email"] = "jane@example.com",
                    ["phone"] = "0987654321"
                }
            }
        };

        _options.Setup(o => o.Value).Returns(config);

        // Act
        var result = _processor.Apply("booking_data", item);

        // Assert
        var contactDetails = (JArray)result["contactDetails"];

        Assert.Equal(2, contactDetails.Count);
        foreach (var contactToken in contactDetails)
        {
            var contact = (JObject)contactToken;
            Assert.False(contact.ContainsKey("email"));
            Assert.True(contact.ContainsKey("phone"));
        }
    }
}
