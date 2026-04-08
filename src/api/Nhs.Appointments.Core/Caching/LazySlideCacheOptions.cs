namespace Nhs.Appointments.Core.Caching;

public record LazySlideCacheOptions<T> : CacheOptions<T>
{
    public LazySlideCacheOptions(Func<Task<T>> updateOperation,
        TimeSpan absoluteExpiration,
        TimeSpan slideThreshold) : base(updateOperation, absoluteExpiration)
    {
        if (absoluteExpiration <= slideThreshold)
        {
            throw new ArgumentException("Configuration is not supported, AbsoluteExpiration must be greater than the SlideThreshold");
        }

        UpdateOperation = updateOperation;
        AbsoluteExpiration = absoluteExpiration;
        SlideThreshold = slideThreshold;
    }

    public Func<Task<T>> UpdateOperation { get; }
    public TimeSpan AbsoluteExpiration { get; }
    public TimeSpan SlideThreshold { get; }
};
