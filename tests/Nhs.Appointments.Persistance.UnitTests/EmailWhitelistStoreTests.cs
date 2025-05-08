using FluentAssertions;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;
public class EmailWhitelistStoreTests
{
    private readonly Mock<ITypedDocumentCosmosStore<WhitelistedEmailDomainsDocument>> _cosmosStore = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly EmailWhitelistStore _sut;

    public EmailWhitelistStoreTests()
    {
        _sut = new EmailWhitelistStore(_cosmosStore.Object, _featureToggleHelper.Object);
    }

    [Fact]
    public async Task ReturnsNhsNetEmailOnly_WhenOktaDisabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.OktaEnabled)).ReturnsAsync(false);

        var whitelistedEmails = await _sut.GetWhitelistedEmails();

        whitelistedEmails.Count().Should().Be(1);
        whitelistedEmails.First().Should().Be("@nhs.net");

        _cosmosStore.Verify(x => x.GetDocument<WhitelistedEmailDomainsDocument>(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task ReturnsWhitelist_AndNhsNet_WhenOktaEnabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.OktaEnabled)).ReturnsAsync(true);
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
