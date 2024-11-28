using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Tests
{
    public class TypeFixDocumentFilterTests
    {
        private readonly TypeFixDocumentFilter _sut;
        public TypeFixDocumentFilterTests() 
        {            
            var testTimeProvider = new TestTimeProvider { Now = new DateTimeOffset(2024, 1, 1, 9, 0, 0, TimeSpan.Zero) };
            _sut = new TypeFixDocumentFilter(typeof(TypeFixDocumentFilterTests).GetNestedTypes(System.Reflection.BindingFlags.NonPublic), testTimeProvider);
        }

        [Theory]
        [InlineData("typeWithDateOnlyProp", "dateOnly", "string", "date", "2024-01-01")]
        [InlineData("typeWithTimeOnlyProp", "timeOnly", "string", "time", "09:00")]
        [InlineData("typeWithDateTimeProp", "dateTime", "string", "date-time", "2024-01-01 09:00")]
        public void Apply_FixesUpTypes(string type, string propTitle, string expectedType, string expectedFormat, string expectedDefault)
        {
            var document = CreateTestDocument(type, propTitle);

            _sut.Apply(null, document);

            document.Components.Schemas.Count.Should().Be(1);
            document.Components.Schemas[type].Properties.Count.Should().Be(1);
            document.Components.Schemas[type].Properties["testProp"].Type.Should().Be(expectedType);
            document.Components.Schemas[type].Properties["testProp"].Format.Should().Be(expectedFormat);
            document.Components.Schemas[type].Properties["testProp"].Default.Should().BeEquivalentTo(new OpenApiString(expectedDefault));
        }

        [Fact]
        public void Apply_FixesUpEnums()
        {
            var type = "typeWithEnumProp";
            var document = CreateTestDocument(type, "");
            _sut.Apply(null, document);
            var expectedValues = new[] { "Alpha", "Beta", "Gamma" };

            document.Components.Schemas.Count.Should().Be(1);
            document.Components.Schemas[type].Properties.Count.Should().Be(1);
            document.Components.Schemas[type].Properties["testProp"].Type.Should().Be("string");
            document.Components.Schemas[type].Properties["testProp"].Enum.Cast<OpenApiString>().Select(x => x.Value).Should().BeEquivalentTo(expectedValues);
            document.Components.Schemas[type].Properties["testProp"].Default.Should().BeEquivalentTo(new OpenApiString("Gamma"));
        }

        [Fact]
        public void Apply_FixesUpDayOfWeekArray()
        {
            var type = "typeWithDayOfWeekArray";
            var document = CreateTestDocument(type, "");
            _sut.Apply(null, document);
            var expectedValues = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            document.Components.Schemas.Count.Should().Be(1);
            document.Components.Schemas[type].Properties.Count.Should().Be(1);
            document.Components.Schemas[type].Properties["testProp"].Type.Should().Be("array");
            document.Components.Schemas[type].Properties["testProp"].Items.Type.Should().Be("string");
            document.Components.Schemas[type].Properties["testProp"].Items.Enum.Cast<OpenApiString>().Select(x => x.Value).Should().BeEquivalentTo(expectedValues);
            document.Components.Schemas[type].Properties["testProp"].Items.Default.Should().BeEquivalentTo(new OpenApiString("Monday"));
        }

        private OpenApiDocument CreateTestDocument(string type, string propTitle) => new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                    {
                        { type, new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema> {
                                    {"testProp", new OpenApiSchema { Type = "object", Title = propTitle }}
                                }
                            }
                        }
                    }
            }
        };
        

        internal record TypeWithDateOnlyProp(DateOnly TestProp);
        internal record TypeWithTimeOnlyProp(TimeOnly TestProp);
        internal record TypeWithDateTimeProp(DateTime TestProp);
        internal record TypeWithEnumProp(TestEnum TestProp);
        internal record TypeWithDayOfWeekArray(DayOfWeek[] TestProp);
    }

    public enum TestEnum
    {
        Alpha,
        Beta,
        Gamma
    }

    public class TestTimeProvider : TimeProvider 
    {
        public DateTimeOffset Now { get; set; }
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
