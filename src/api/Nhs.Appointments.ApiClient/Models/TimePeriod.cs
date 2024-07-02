namespace Nhs.Appointments.ApiClient.Models
{
    public class TimePeriod
    {
        public TimePeriod(DateTime from, DateTime until)
        {
            if (until <= from)
                throw new ArgumentException("Start must be earlier than finish");
            From = from;
            Duration = until - from;
        }

        public TimePeriod(DateTime from, TimeSpan duration)
        {
            From = from;
            Duration = duration;
        }

        public DateTime From { get; private set; }

        public TimeSpan Duration { get; private set; }
        internal DateTime Until => From.Add(Duration);
    }
}
