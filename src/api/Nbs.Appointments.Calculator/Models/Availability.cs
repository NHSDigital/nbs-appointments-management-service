using System.Linq;
using System.Net.Http.Headers;

namespace Nbs.Appointments.Calculator.Models
{
    public class Availability
    {
        public Availability(string[] services, int capacity)
        {
            Services = services;
            Capacity = capacity;
            Id = Guid.NewGuid();
        }

        private string[] _booked;

        public string[] Services { get; }

        public int Capacity { get; }

        public Guid Id { get; }

        public string[] Booked { get => _booked is null ? [] : _booked; }

        public int RemainingCapacity => Capacity - Booked.Count();

        public string[] TryAddBookings(string[] bookings) 
        {
            var applicableBookings = bookings.Where(x => Services.Contains(x));
            var notapplicableBookings = bookings.Where(x => !Services.Contains(x)).ToList();

            if (applicableBookings.Count() <= 0) 
            {
                return bookings;
            }
            var stack = new Stack<string>(applicableBookings);
            var selectedBookings = PopRange(stack, Math.Min(RemainingCapacity, applicableBookings.Count()));
            _booked = selectedBookings.ToArray();

            notapplicableBookings.AddRange(stack);
            return notapplicableBookings.ToArray();
        }

        private List<string> PopRange(Stack<string> stack, int amount)
        {
            var result = new List<string>(amount);
            while (amount-- > 0 && stack.Count > 0)
            {
                result.Add(stack.Pop());
            }
            return result;
        }
    }

    public class LinkedAvailability
    {
        public LinkedAvailability(Availability availability, IEnumerable<Availability> possibleLinks)
        {
            Availability = availability;

            var links = possibleLinks.Where(
                pl => pl.Services.Any(
                    service => Availability.Services.Contains(service)));
            LinkedAvailabilities = links.Select(
                linked => new LinkedAvailability(linked, possibleLinks.Where(x => !linked.Id.Equals(x.Id) && linked.Services.Any(service => x.Services.Contains(service)))));
        }

        public (string[] RemainingServices, int RemainingCapacity) TryBookService(string priorityService, string[] bookedServices) 
        {
            var branchResults = new List<(string[] RemainingServices, int RemainingCapacity)>();
            foreach (var availibility in LinkedAvailabilities) 
            {
                branchResults.Add(availibility.TryBookService(priorityService, bookedServices));
            }

            if (LinkedAvailabilities.Count() > 0)
            {
                bookedServices = branchResults.OrderBy(x => x.RemainingServices.Count(service => service.Equals(priorityService)))?.FirstOrDefault().RemainingServices ?? bookedServices;
            }

            var applicableBookings = new Stack<string>(bookedServices.Where(x => Availability.Services.Contains(x)).OrderBy(x => x.Equals(priorityService) ? 0 : 1));
            var notapplicableBookings = bookedServices.Where(x => !Availability.Services.Contains(x)).ToList();
            var bookedCount = Math.Min(Availability.Capacity, applicableBookings.Count());

            var _ = PopRange(applicableBookings, Math.Min(Availability.Capacity, applicableBookings.Count()));

            notapplicableBookings.AddRange(applicableBookings);
            return (RemainingServices: notapplicableBookings.ToArray(), RemainingCapacity: Availability.Capacity - applicableBookings.Count());
        }

        public void PrintToConsole() 
        {
            Console.WriteLine($"{string.Join(",", Availability.Services)}");

            foreach(var linked in LinkedAvailabilities) { linked.PrintToConsole(); }
        }

        private List<string> PopRange(Stack<string> stack, int amount)
        {
            var result = new List<string>(amount);
            while (amount-- > 0 && stack.Count > 0)
            {
                result.Add(stack.Pop());
            }
            return result;
        }

        public IEnumerable<Availability> GetAvailabilities() => new List<Availability>(LinkedAvailabilities.SelectMany(x => x.GetAvailabilities())) { Availability };

        public Availability Availability { get; set; }
        public IEnumerable<LinkedAvailability> LinkedAvailabilities { get; set; }
    }
}
