using System.Linq;

namespace Nbs.Appointments.Calculator.Models
{
    public class Availability
    {
        public Availability(string[] services, int capacity)
        {
            Services = services;
            Capacity = capacity;
        }

        private string[] _booked;

        public string[] Services { get; }

        public int Capacity { get; }

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
}
