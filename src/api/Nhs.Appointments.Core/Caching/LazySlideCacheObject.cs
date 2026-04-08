namespace Nhs.Appointments.Core.Caching;

internal record LazySlideCacheObject<T>(T Value, DateTimeOffset DateTimeUpdated)
{
    public bool IsFresh(TimeSpan slideThreshold, DateTimeOffset now) => DateTimeUpdated.Add(slideThreshold) >= now;
}
