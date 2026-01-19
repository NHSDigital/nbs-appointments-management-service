using FluentAssertions;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;
public class EmailWhitelistStoreTests
{
    private readonly Mock<ITypedDocumentCosmosStore<WhitelistedEmailDomainsDocument>> _cosmosStore = new();
    private readonly EmailWhitelistStore _sut;

    public EmailWhitelistStoreTests()
    {
        _sut = new EmailWhitelistStore(_cosmosStore.Object);
    }

    [Fact]
    public async Task ReturnsWhitelist_AndNhsNet()
    {
        _cosmosStore.Setup(x => x.GetDocument<WhitelistedEmailDomainsDocument>(It.IsAny<string>()))
            .ReturnsAsync(new WhitelistedEmailDomainsDocument
            {
                Domains = ["@nhs.net", "@test-domain.com", "@another-test-domain.co.uk"]
            });

        var whitelistedEmails = await _sut.GetWhitelistedEmails();

        whitelistedEmails.Count().Should().Be(3);
        whitelistedEmails.Should().Contain("@nhs.net");

        _cosmosStore.Verify(x => x.GetDocument<WhitelistedEmailDomainsDocument>("whitelisted_email_domains"), Times.Once());
    }
}
