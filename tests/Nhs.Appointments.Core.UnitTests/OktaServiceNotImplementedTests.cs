using Nhs.Appointments.Core.Okta;

namespace Nhs.Appointments.Core.UnitTests;

public class OktaServiceNotImplementedTests
{
    private readonly OktaService _sut;
    private readonly Mock<TimeProvider> _timeProvider = new();

    private readonly string userEmail = "test@okta.com";
    private readonly string firstName = "first";
    private readonly string lastName = "last";

    public OktaServiceNotImplementedTests() =>
        _sut = new OktaService(new OktaUnimplementedUserDirectory(), _timeProvider.Object);

    [Fact(DisplayName =
        "Always throws a NotImplementedException if the IOktaUserDirectory injects the unimplemented store")]
    public async Task CreateIfNotExists_ThrowsNotImplementedException()
    {
        await Assert.ThrowsAsync<NotImplementedException>(async () =>
            await _sut.CreateIfNotExists(userEmail, firstName, lastName)
        );
    }
}
