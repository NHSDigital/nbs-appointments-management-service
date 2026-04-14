using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core.UnitTests.Concurrency;

public class SiteLeaseContextTests
{
    [Fact]
    public void SiteKeyNotSupplied_ConstructorCalled_ThrowsArgumentNullException()
    {
        // Arrange.
        Action releaseAction = () => { };

        Action action = () => new SiteLeaseContext("", releaseAction);

        // Act - not required.

        // Assert.
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'siteKey')");
    }

    [Fact]
    public void ReleaseActionNotSupplied_ConstructorCalled_ThrowsArgumentNullException()
    {
        // Arrange.
        Action action = () => new SiteLeaseContext("siteKey", null);

        // Act - not required.

        // Assert.
        action.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'release')");
    }

    [Fact]
    public void SiteLeaseContextCreated_SiteKeyRequested_ReturnsCorrectValue()
    {
        // Arrange.
        Action releaseAction = () => { };
        var randomSiteKey = Guid.NewGuid().ToString();
        var sut = new SiteLeaseContext(randomSiteKey, releaseAction);

        // Act - not required.

        // Assert.
        sut.SiteKey.Should()
            .Be(randomSiteKey);
    }

    [Fact]
    public void ReleaseActionSet_SiteLeaseContextDisposed_ReleaseActionCalled()
    {
        // Arrange.
        var releaseAction = new Mock<Action>();
        var randomSiteKey = Guid.NewGuid().ToString();

        // Act.
        var sut = new SiteLeaseContext(randomSiteKey, releaseAction.Object);
        sut.Dispose();

        // Assert.
        releaseAction.Verify(action => action(), Times.Once());
    }
}
