using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Availability;

namespace Nhs.Appointments.Api.Tests.Validators;

public class QueryAvailabilityRequestValidatorTests
{
    private const string InvalidSitesArrayErrorMessage = "One or more site identifiers must be provided";
    private const string InvalidSiteIdentifierErrorMessage = "All provided site identifiers must be valid";
    private const string InvalidStringErrorMessage = "Provide a valid string";
    private const string InvalidDateErrorMessage = "Provide a date in the format 'yyyy-MM-dd'";
    private const string InvalidFromDateRangeErrorMessage = "From date must be on or before Until date";
    private const string InvalidRequestBodyErrorMessage = "A request body must be provided";
    private const string InvalidQueryTypeValueMessage = "Value must be one of: Days, Hours, Slots";
    private readonly QueryAvailabilityRequestValidator _sut = new();
    
    [Fact]
    public void Validate_ReturnsValidResult_WhenAllFieldsAreValid()
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            "COVID",
            "2077-01-01",
            "2077-01-01",
            QueryType.Days
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(new string[] {}, InvalidSitesArrayErrorMessage)]
    [InlineData(null, InvalidSitesArrayErrorMessage)]
    [InlineData(new [] {""}, InvalidSiteIdentifierErrorMessage)]
    [InlineData(new string[] {null}, InvalidSiteIdentifierErrorMessage)]
    [InlineData(new [] {"1000", null}, InvalidSiteIdentifierErrorMessage)]
    [InlineData(new [] {"", "1000"}, InvalidSiteIdentifierErrorMessage)]
    public void Validate_ReturnsError_WhenSitesIsNullOrEmpty(string[] sites, string message)
    {
        var request = new QueryAvailabilityRequest(
            sites,
            "SERVICE",
            "2077-01-01",
            "2077-01-01",
            QueryType.Days
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(QueryAvailabilityRequest.Sites));
        result.Errors.Single().ErrorMessage.Should().Be(message);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ReturnsError_WhenServiceIsNullOrEmpty(string service)
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            service,
            "2077-01-01",
            "2077-01-01",
            QueryType.Days
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryAvailabilityRequest.Service));
        result.Errors.Single().ErrorMessage.Should().Be(InvalidStringErrorMessage);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("01-01-2077")]
    [InlineData("2077-99-31")]
    [InlineData("2077-01-99")]
    [InlineData("Not a date")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenFromDateIsNotValid(string fromDate)
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            "COVID",
            fromDate,
            "2077-01-01",
            QueryType.Days
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryAvailabilityRequest.From));
        result.Errors.Single().ErrorMessage.Should().Be(InvalidDateErrorMessage);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("01-01-2077")]
    [InlineData("2077-99-31")]
    [InlineData("2077-01-99")]
    [InlineData("Not a date")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenUntilDateIsNotValid(string untilDate)
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            "COVID",
            "2077-01-01",
            untilDate,
            QueryType.Days
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryAvailabilityRequest.Until));
        result.Errors.Single().ErrorMessage.Should().Be(InvalidDateErrorMessage);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenUntilDateIsBeforeFrom()
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            "COVID",
            "2077-01-01",
            "2076-01-01",
            QueryType.Days
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryAvailabilityRequest.FromDate));
        result.Errors.Single().ErrorMessage.Should().Be(InvalidFromDateRangeErrorMessage);
    }
    
    [Theory]
    [InlineData(QueryType.Days)]
    [InlineData(QueryType.Hours)]
    [InlineData(QueryType.Slots)]
    public void Validate_ReturnsValidResult_WhenQueryTypeIsInRange(QueryType queryType)
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            "COVID",
            "2077-01-01",
            "2077-01-01",
            queryType
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(QueryType.NotSet)]
    public void Validate_ReturnsError_WhenQueryTypeIsNotInRange(QueryType queryType)
    {
        var request = new QueryAvailabilityRequest(
            new [] {"1000", "1001"},
            "COVID",
            "2077-01-01",
            "2077-01-01",
            queryType
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(QueryAvailabilityRequest.QueryType));
        result.Errors.Single().ErrorMessage.Should().Be(InvalidQueryTypeValueMessage);    
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenRequestBodyIsNull()
    {
        QueryAvailabilityRequest request = null;
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().ErrorMessage.Should().Be(InvalidRequestBodyErrorMessage);
    }    
}