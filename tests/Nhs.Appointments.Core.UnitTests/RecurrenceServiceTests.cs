using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core.UnitTests;

public class RecurrenceServiceTests
{
    private readonly RecurrenceService _sut =  new RecurrenceService();
    
    [Fact] 
    public void Recurrence_Test()
    {
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        Session[] sessions =
        [
            new()
            {
                Capacity = 1,
                From = new TimeOnly(09, 00),
                Until = new TimeOnly(10, 00),
                SlotLength = 5,
                Services = ["Service 1"]
            }
        ];

        var template = new Template
        {
            Days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Saturday, DayOfWeek.Sunday], Sessions = sessions
        };

        var occurrences = _sut.GenerateStandardOccurrences( from, until, template).ToList();
        
        occurrences.Should().HaveCount(4);
        // occurrences[0].Source.Start.Should().Be()
        
        // var expectedDates = new[]
        // {
        //     new DateOnly(2025, 01, 06), new DateOnly(2025, 01, 07), new DateOnly(2025, 01, 11),
        //     new DateOnly(2025, 01, 12)
        // };
        
        var movedOccurrences = _sut.GenerateMovedOccurrences( from, until, template).ToList();
        
        movedOccurrences.Should().HaveCount(4);
    }
}
