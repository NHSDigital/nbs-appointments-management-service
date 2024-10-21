namespace Nhs.Appointments.Core.UnitTests;

public class AvailabilityServiceTests
{
    private readonly AvailabilityService _sut;
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();


    public AvailabilityServiceTests()
    {
        _sut = new AvailabilityService(_availabilityStore.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfSiteIsEmpty(string? siteId)
    {
        var site = siteId;
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>().WithMessage("site must have a value");
        
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfTemplateIsNull()
    {
        const string site = "ABC01";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        Template template = null;
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>().WithMessage("template must be provided");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfFromDateIsAfterUntilDate()
    {
        const string site = "ABC01";
        var from = new DateOnly(2025, 01, 02);
        var until = new DateOnly(2025, 01, 01);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>().WithMessage("until date must be after from date");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfNoDaysAreSpecified()
    {
        var site = "ABC01";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>("template must specify one or more weekdays");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfTemplateContainsNoSessions()
    {
        var site = "ABC01";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions = []
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>("template must contain one or more sessions");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_CallsAvailabilityStore_WithDatesForRequestedDays()
    {
        var site = "ABC01";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Saturday, DayOfWeek.Sunday],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        
        await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template);
        var actualDates = _availabilityStore.Invocations
            .Where(i => i.Method.Name == nameof(IAvailabilityStore.ApplyAvailabilityTemplate))
            .Select(i => (DateOnly)i.Arguments[1]);
        var expectedDates = new[]
        {
            new DateOnly(2025, 01, 06),
            new DateOnly(2025, 01, 07),
            new DateOnly(2025, 01, 11),
            new DateOnly(2025, 01, 12)
        };
        actualDates.Should().BeEquivalentTo(expectedDates);
    }
}
