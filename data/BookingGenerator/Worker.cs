using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;

namespace BookingGenerator;

public class Worker(ISiteStore siteStore, IClinicalServiceStore clinicalServiceStore, IAvailabilityService availabilityService, IBookingsService bookingsService, IConfiguration configuration, TimeProvider timeProvider, ILogger<Worker> logging) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var numberOfClinicalServices = configuration.GetValue<int>("NUMBER_OF_CLINICALSERVICES");
        var monthsBack = configuration.GetValue<int>("MONTHS_BACK");
        var monthsForward = configuration.GetValue<int>("MONTHS_FORWARD");
        
        var sites = siteStore.GetAllSites();
        var clinicalServices = GetClinicalServices(numberOfClinicalServices);

        await Task.WhenAll(sites, clinicalServices);

        var today = timeProvider.GetUtcNow();
        var startDate = today.AddMonths(-monthsBack);
        var endDate = today.AddMonths(monthsForward);

        var days = GenerateDays(startDate, endDate);

        foreach (var day in days)
        {
            logging.LogInformation($"Starting Seeding for Day {day:dd-MM-yyyy}");

            var tasks = sites.Result.Select(site => SeedForSite(site.Id, day, clinicalServices.Result));

            foreach (var batch in tasks.Batch(10).Select(batch => batch))
            {
                await Task.WhenAll(batch);
            }
            
            logging.LogInformation($"Finished Seeding for Day {day:dd-MM-yyyy}");
        }
        
        logging.LogInformation($"Finished Seeding for all days {startDate:dd-MM-yyyy} - {endDate:dd-MM-yyyy}");
    }

    private async Task SeedForSite(string id, DateOnly day, IEnumerable<string> clinicalServices)
    {
        if (SkipDay())
        {
            logging.LogInformation("Skipped Day");
            return;
        }
        
        logging.LogInformation($"Starting to seed for Site {id} - {day.ToLongDateString()}");
        
        var sessions = Enumerable.Range(0, RandomNumberBetween(1, 3))
            .Select(x => new Session
            {
                Capacity = RandomNumberBetween(1, 10),
                From = new TimeOnly(RandomNumberBetween(8, 12), 0),
                Until = new TimeOnly(RandomNumberBetween(13, 17), 0),
                Services = clinicalServices.OrderBy(_ => Guid.NewGuid())
                    .Take(RandomNumberBetween(1, clinicalServices.Count()))
                    .Select(service => service).ToArray(),
                SlotLength = RandomNumberBetween(1, 10)
            }).ToArray();
            
            await TryCosmosTask(availabilityService.ApplySingleDateSessionAsync(day, id, sessions, ApplyAvailabilityMode.Overwrite, "BookingGenerator"));

            foreach (var slot in sessions.Select(
                s => new SessionInstance(day.ToDateTime(s.From),
                    day.ToDateTime(s.Until))
                {
                    Services = s.Services, SlotLength = s.SlotLength, Capacity = s.Capacity
                }).SelectMany(x => x.ToSlots()).OrderBy(_ => Guid.NewGuid()))
            {
                await AddBookings(id, slot);
            }
    }

    private async Task AddBookings(string site, SessionInstance slot)
    {
        var bookings = Enumerable.Range(0, RandomNumberBetween(1, slot.Capacity)).Select(x => new Booking()
        {
            Service = slot.Services.OrderBy(_ => Guid.NewGuid()).First(),
            Site = site,
            Duration = (int)slot.Duration.TotalMinutes, 
            From = slot.From,
            Status = AppointmentStatus.Booked,
            AttendeeDetails = new AttendeeDetails()
            {
                DateOfBirth = new DateOnly(2000,1,1),
                FirstName = "John",
                LastName = "Doe",
                NhsNumber = "NHSNUMBER"
            },
            ContactDetails =
            [
                new ContactItem()
                {
                    Type = ContactItemType.Email,
                    Value = "Some Contact Stuff"
                },
                new ContactItem()
                {
                    Type = ContactItemType.Landline,
                    Value = "Some Contact Stuff"
                },
                new ContactItem()
                {
                    Type = ContactItemType.Phone,
                    Value = "Some Contact Stuff"
                }
            ]
        });

        var bookingTasks = bookings.Select(booking => TryCosmosTask(MakeBooking(booking)));

        await Task.WhenAll(bookingTasks);
    }

    private async Task MakeBooking(Booking booking)
    {
        if (SkipBooking())
        {
            logging.LogInformation("Skipped Booking");
            return;
        }
        
        var result = await bookingsService.MakeBooking(booking);
            
        logging.LogInformation($"Booking for {booking.Site} on {booking.From:dd-MM-yyyy hh:mm} result - {(result.Success ? result.Reference : "Failed")}");
    }

    private static IEnumerable<DateOnly> GenerateDays(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        return Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
            .Select(offset => startDate.AddDays(offset))
            .Select(datetime => new DateOnly(datetime.Year, datetime.Month, datetime.Day));
    }

    private static bool SkipDay()
    {
        return RandomNumberBetween(1, 5) == 1;
    }
    
    private static bool SkipBooking()
    {
        return RandomNumberBetween(1, 10) == 1;
    }

    private async Task TryCosmosTask(Task cosmosTask, int attempts = 1)
    {
        try
        {
            await cosmosTask;
        }
        catch (Exception ex)
        {
            if (attempts > 50)
            {
                throw;
            }
            logging.LogInformation($"Delaying Cosmos Task {attempts} times due to {ex.Message}");
            await Task.Delay(30000);
            await TryCosmosTask(cosmosTask, attempts + 1);
        }
    }

    private static int RandomNumberBetween(int min, int max)
    {
        var random = new Random();
        return random.Next(min, max);
    }

    private async Task<IEnumerable<string>> GetClinicalServices(int numberOfServices)
    {
        var services = await clinicalServiceStore.Get();

        return services.Take(numberOfServices).Select(x => x.Value);
    }
}

public static class Extensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
    {
        return items.Select((item, idx) => new { item, idx })
            .GroupBy(x => x.idx / maxItems)
            .Select(g => g.Select(x => x.item));
    }
}
