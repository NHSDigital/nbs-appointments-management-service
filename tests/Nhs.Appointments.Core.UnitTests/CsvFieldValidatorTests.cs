namespace Nhs.Appointments.Core.UnitTests;

public class CsvFieldValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void StringHasValueReturnsFalse(string value)
    {
        var result = CsvFieldValidator.StringHasValue(value);

        result.Should().BeFalse();
    }

    [Fact]
    public void StringHasValueReturnsTrue()
    {
        var result = CsvFieldValidator.StringHasValue("test");

        result.Should().BeTrue();
    }

    [Fact]
    public void ParseUserEnteredBoolean_ThrowsFormatException()
    {
        const string value = "test";
        var action = () => CsvFieldValidator.ParseUserEnteredBoolean(value);

        action.Should().Throw<FormatException>().WithMessage($"Invalid bool string format: {value}");
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void ParseUserEnteredBoolean_ReturnsParsedValue(string boolAsString)
    {
        var expectedResult = bool.Parse(boolAsString);

        var result = CsvFieldValidator.ParseUserEnteredBoolean(boolAsString);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("N")]
    [InlineData("01234 856856")]
    public void ShouldReturnTrueForValidPhoneNUmber(string phoneNumber)
    {
        var result = CsvFieldValidator.IsValidPhoneNumber(phoneNumber);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("test string")]
    public void ShouldReturnFalseForValidPhoneNumber(string? phoneNumber)
    {
        var result = CsvFieldValidator.IsValidPhoneNumber(phoneNumber);

        result.Should().BeFalse();
    }
}
