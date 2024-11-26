using FluentAssertions;
using Nhs.Appointments.Api.Notifications;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class PrivacyUtilTests
{
    private PrivacyUtil _sut = new();

    [Fact]
    public void CanObfuscateEmails()
    {
        _sut.ObfuscateEmail("test@server.com").Should().Be("t**t@server.com");
    }


    [Fact]
    public void EmailCanBeNull()
    {
        _sut.ObfuscateEmail(null).Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("07654 123456", "********3456")]
    [InlineData("123456", "**3456")]
    [InlineData("3456", "3456")]
    [InlineData("56", "56")]
    public void CanObfuscatePhoneNumbers(string input, string expected)
    {
        _sut.ObfuscatePhoneNumber(input).Should().Be(expected);
    }

    [Fact]
    public void PhoneNumberCanBeNull()
    {
        _sut.ObfuscatePhoneNumber(null).Should().Be(string.Empty);
    }
}
