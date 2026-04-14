using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core.UnitTests.Concurrency;

public class LeaseKeysTests
{
    [Fact]
    public void SiteIdIsNull_SiteKeyIsCreated_ThrowsArgumentNullException()
    {
        // Arrange - not required.

        // Act.
        Action action = () => LeaseKeys.SiteKeyFactory.Create(null, DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert.
        action.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'siteId')");
    }

    [Fact]
    public void SiteIdIsEmpty_SiteKeyIsCreated_ThrowsArgumentException()
    {
        // Arrange - not required.

        // Act.
        Action action = () => LeaseKeys.SiteKeyFactory.Create("", DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert.
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'siteId')");
    }

    [Theory]
    [MemberData(nameof(MemberDataForSiteLeaseFactory))]
    public void SiteAndDateSupplied_SiteKeyIsCreated_ReturnsCorrectValue(string siteId, DateOnly date, string expectedKey)
    {
        // Arrange - not required.

        // Act - not required.
        
        // Assert.
        LeaseKeys.SiteKeyFactory.Create(siteId, date).Should().Be(expectedKey);
    }

    public static IEnumerable<object[]> MemberDataForSiteLeaseFactory()
    {
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();
        var guid3 = Guid.NewGuid().ToString();

        return new List<object[]> {
            new object[] { guid1, new DateOnly(2026, 08, 14), $"{guid1}_20260814" },
            new object[] { guid2, new DateOnly(2026, 12, 31), $"{guid2}_20261231" },
            new object[] { guid3, new DateOnly(2028, 02, 29), $"{guid3}_20280229" }
        };
    }
}
