using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators
{
    public class SetTemplateAssignmentRequestValidatorTests
    {
        private readonly SetTemplateAssignmentRequestValidator _sut = new();

        [Fact]
        public void Validate_EnsuresSiteIsProvided()
        {
            var testData = new SetTemplateAssignmentRequest()
            {
                Assignments = new TemplateAssignment[0]
            };
            var result = _sut.Validate(testData);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetTemplateAssignmentRequest.Site));
            result.Errors.Single().ErrorMessage.Should().Be("Provide a site");
        }

        [Fact]
        public void Validate_EnsuresAssignments_IsNotNull()
        {
            var testData = new SetTemplateAssignmentRequest()
            {
                Site = "123"
            };
            var result = _sut.Validate(testData);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetTemplateAssignmentRequest.Assignments));
            result.Errors.Single().ErrorMessage.Should().Be("Assignments must be specified");
        }

        [Fact]
        public void Validate_AllowsEmptyAssignments()
        {
            var testData = new SetTemplateAssignmentRequest()
            {
                Site = "123",
                Assignments = new TemplateAssignment[0]
            };
            var result = _sut.Validate(testData);
            result.IsValid.Should().BeTrue();            
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Validate_EnsuresAllAssignments_HaveATemplateId(string templateId)
        {
            var testData = new SetTemplateAssignmentRequest
            {
                Site = "2124",
                Assignments = new[]
                {
                    new TemplateAssignment("2024-04-11", "2024-04-12", templateId)
                }
            };
            var result = _sut.Validate(testData);            
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetTemplateAssignmentRequest.Assignments));
            result.Errors.Single().ErrorMessage.Should().Be("All assignments must have a valid template id");
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "2024-01-24")]
        [InlineData("2024-24-01", "")]
        [InlineData("bad", "2024-01-24")]
        [InlineData("2024-01-24", "bad")]
        [InlineData("2024-24-01", "2024-01-24")]
        public void Validate_EnsuresAllAssignment_HaveValidDates(string from, string until)
        {
            var testData = new SetTemplateAssignmentRequest
            {
                Site = "2124",
                Assignments = new[]
               {
                    new TemplateAssignment(from, until, "1213")
                }
            };
            var result = _sut.Validate(testData);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetTemplateAssignmentRequest.Assignments));
            result.Errors.Single().ErrorMessage.Should().Be("Assignments must have valid dates provided in the format 'yyyy-MM-dd'");            
        }

        [Theory]
        [InlineData("2024-03-20", "2024-04-02")] // end is overlapping
        [InlineData("2024-03-20", "2024-04-01")] // end is overlapping boundary
        [InlineData("2024-04-09", "2024-04-14")] // start is overlapping
        [InlineData("2024-04-10", "2024-04-14")] // start is overlapping boundary
        [InlineData("2024-04-02", "2024-04-09")] // is contained by
        [InlineData("2024-04-01", "2024-04-10")] // is same
        public void Validate_EnsuresThereAreNoOverlappingDates(string from, string until)
        {
            var testData = new SetTemplateAssignmentRequest
            {
                Site = "2124",
                Assignments = new[]
                {                    
                    new TemplateAssignment("2024-04-01", "2024-04-10", "123"),
                    new TemplateAssignment(from, until, "123")
                }
            };
            var result = _sut.Validate(testData);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetTemplateAssignmentRequest.Assignments));
            result.Errors.Single().ErrorMessage.Should().Be("Assignments cannot contain overlapping date periods");
        }

        [Fact]
        public void Validate_EnsureThatDateRangesAreValid()
        {
            var testData = new SetTemplateAssignmentRequest
            {
                Site = "2124",
                Assignments = new[]
                {
                    new TemplateAssignment("2024-04-10", "2024-04-01", "123"),                    
                }
            };
            var result = _sut.Validate(testData);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(nameof(SetTemplateAssignmentRequest.Assignments));
            result.Errors.Single().ErrorMessage.Should().Be("All assignments must have valid date ranges");
        }
    }
}
