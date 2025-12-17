using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;
using Nhs.Appointments.Core.Caching;

namespace Nhs.Appointments.Core.UnitTests;

public class CacheServiceTests
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly CacheService _sut;
    private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
    
    public CacheServiceTests()
    {
        _sut = new CacheService(_memoryCache, _timeProvider);
    }

    private int FakeExpensiveBoolOperationCallCount { get; set; }

    private TimeSpan ExpensiveOperationTimespan => TimeSpan.FromMinutes(30);

    private TimeSpan DefaultSlideThreshold => TimeSpan.FromHours(10);

    private TimeSpan DefaultCacheExpiration => TimeSpan.FromHours(60);

    private string DefaultCacheKey => "ACacheKey";

    private LazySlideCacheOptions<bool> DefaultOptions => new(FakeExpensiveTrueOperation, DefaultSlideThreshold, DefaultCacheExpiration);

    private async Task<bool> FakeExpensiveTrueOperation()
    {
        return await FakeExpensiveBoolOperation(true);
    }

    private async Task<bool> FakeExpensiveFalseOperation()
    {
        return await FakeExpensiveBoolOperation(false);
    }

    private async Task<bool> FakeExpensiveBoolOperation(bool operationResult)
    {
        FakeExpensiveBoolOperationCallCount++;

        var tcs = new TaskCompletionSource<bool>();
        _timeProvider.CreateTimer(_ =>
        {
            tcs?.TrySetResult(operationResult);
        }, null, ExpensiveOperationTimespan, Timeout.InfiniteTimeSpan);

        tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return await tcs.Task;
    }

    [Fact]
    public async Task LazySlidingCacheValuesSet()
    {
        var cacheValue = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        Assert.False(cacheValue.IsCompleted);

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue1);
        keyValue1.Should().BeNull();

        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        (await cacheValue).Should().BeTrue();
        Assert.True(cacheValue.IsCompleted);
        
        //Need to query by lazy slide prefix
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue2);
        keyValue2.Should().BeNull();

        //Need to query by lazy slide prefix
        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue3);
        ((CacheService.LazySlideCacheObject)keyValue3)?.Value.Should().Be(true);
    }
    
    [Fact]
    public async Task Standard_Caching_Behaviour()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        var call2 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        var call3 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        Task.WhenAll(call1, call2, call3);
        
        call1.Result.Should().BeTrue();
        call2.Result.Should().BeTrue();
        call3.Result.Should().BeTrue();

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        FakeExpensiveBoolOperationCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DifferentKeys_Concurrent_CacheValuesSetIndependently()
    {
        var call1 = _sut.GetLazySlidingCacheValue("Key1", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call2 = _sut.GetLazySlidingCacheValue("Key2", new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call3 = _sut.GetLazySlidingCacheValue("Key3", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        
        //all expensive operations performed concurrently
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        Task.WhenAll(call1, call2, call3);
        
        call1.Result.Should().BeTrue();
        call2.Result.Should().BeFalse();
        call3.Result.Should().BeTrue();

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key1"), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key2"), out var keyValue2);
        ((CacheService.LazySlideCacheObject)keyValue2)?.Value.Should().Be(false);
        
        _memoryCache.TryGetValue("Key3", out var keyValue3);
        ((CacheService.LazySlideCacheObject)keyValue3)?.Value.Should().Be(true);
        
        //all 3 trigger
        FakeExpensiveBoolOperationCallCount.Should().Be(3);
    }

    /// <summary>
    ///     Update the cache value silently if it exists and the slide threshold has been passed
    /// </summary>
    [Fact]
    public async Task Standard_Lazy_Sliding_Behaviour()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        (await call1).Should().BeTrue();

        FakeExpensiveBoolOperationCallCount.Should().Be(1);

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        //slide threshold has been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Add(TimeSpan.FromMinutes(1)));

        //the expensive operation now returns a DIFFERENT RESULT.
        //this cache value should be slid in the background for the next request, but this first usage returns the old value (lazy)
        var call2 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation, DefaultSlideThreshold, DefaultCacheExpiration));

        //this await is QUICK, as it does not await the update slide outcome, it just returns the current cache value

        //the call returns the CURRENT outdated cached value
        (await call2).Should().BeTrue();

        //slide threshold has been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Add(TimeSpan.FromMinutes(1)));

        //the slide was invoked so the count increases
        FakeExpensiveBoolOperationCallCount.Should().Be(2);

        //some better way of asserting a separate thread has updated this value without hacky wait checks??
        var slideCacheValue1 = true;
        while (slideCacheValue1)
        {
            await Task.Delay(5);
            _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue2);
            slideCacheValue1 = (bool)((CacheService.LazySlideCacheObject)keyValue2)?.Value!;
        }

        Assert.False(slideCacheValue1);

        var call3 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        //the next call gets the previously updated cache value
        (await call3).Should().BeFalse();

        //since this call operation has set it back to true, the cache should have reverted back

        //expensive operation that changed the flag has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        //the slide was invoked so the count increases
        FakeExpensiveBoolOperationCallCount.Should().Be(3);

        //some better way of asserting a separate thread has updated this value without hacky wait checks??
        var slideCacheValue2 = false;
        while (!slideCacheValue2)
        {
            await Task.Delay(5);
            _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue3);
            slideCacheValue2 = (bool)(keyValue3 as CacheService.LazySlideCacheObject)?.Value!;
        }

        Assert.True(slideCacheValue2);
    }

    /// <summary>
    ///     No sliding if below threshold
    /// </summary>
    [Fact]
    public async Task Standard_Lazy_Sliding_Behaviour_NotTriggered_Below_Threshold()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        (await call1).Should().BeTrue();

        FakeExpensiveBoolOperationCallCount.Should().Be(1);

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        //slide threshold has NOT YET been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Subtract(ExpensiveOperationTimespan)
            .Subtract(TimeSpan.FromMinutes(10)));

        //second request WOULD have returned a new updated cache value, but we haven't hit the threshold yet
        var call2 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation, DefaultSlideThreshold, DefaultCacheExpiration));

        //this await is QUICK, as it does not await the update slide outcome, it just returns the current cache value

        //the call returns the CURRENT outdated cached value
        (await call2).Should().BeTrue();

        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(10)));

        //the count has not incremented as the slide was not invoked!
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
    }

    [Fact]
    public async Task MultipleCallsWithinTimeframe_DoNotTriggerMultipleExpensiveOperations_Expiration()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        FakeExpensiveBoolOperationCallCount.Should().Be(1);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        (await call1).Should().BeTrue();
        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        //No easy way to simulate timeProvider with memoryCacheOptions
        //Just simulate the cache expiring
        _memoryCache.Remove(CacheService.LazySlideCacheKey(DefaultCacheKey));

        var call2 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        (await call2).Should().BeTrue();
        Assert.True(call2.IsCompleted);

        FakeExpensiveBoolOperationCallCount.Should().Be(2);
    }

    /// <summary>
    ///     If multiple requests are within the Slide threshold window, it does a SINGLE slide operation
    ///     All the requests will return the outdated value until the cache is updated
    /// </summary>
    [Fact]
    public async Task MultipleCallsWithinTimeframe_DoNotTriggerMultipleExpensiveOperations_SlideThreshold()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        FakeExpensiveBoolOperationCallCount.Should().Be(1);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        (await call1).Should().BeTrue();

        //slide threshold has been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Add(TimeSpan.FromMinutes(1)));

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey(DefaultCacheKey), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        //multiple requests occur during the slide threshold
        var call2 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        //only call2 should trigger a single update operation
        var call3 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        var call4 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);
        var call5 = _sut.GetLazySlidingCacheValue(DefaultCacheKey, DefaultOptions);

        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        Task.WaitAll(call2, call3, call4, call5);

        call2.Result.Should().BeTrue();
        call3.Result.Should().BeTrue();
        call4.Result.Should().BeTrue();
        call5.Result.Should().BeTrue();

        FakeExpensiveBoolOperationCallCount.Should().Be(2);
    }
    
    /// <summary>
    ///     If multiple requests are within the Slide threshold window for different keys
    /// </summary>
    [Fact]
    public async Task DifferentKeys_MultipleCallsWithinTimeframe_DoNotTriggerMultipleExpensiveOperations_SlideThreshold()
    {
        //initially only 2 requests called
        var call1 = _sut.GetLazySlidingCacheValue("Key1", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call2 = _sut.GetLazySlidingCacheValue("Key2", new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        
        //slide threshold passed
        _timeProvider.Advance(DefaultSlideThreshold.Add(TimeSpan.FromMinutes(1)));
        
        Task.WhenAll(call1, call2);
        
        call1.Result.Should().BeTrue();
        call2.Result.Should().BeFalse();

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key1"), out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);

        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key2"), out var keyValue2);
        ((CacheService.LazySlideCacheObject)keyValue2)?.Value.Should().Be(false);
        
        _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key3"), out var keyValue3);
        keyValue3.Should().BeNull();
        
        //only 2 trigger initially
        FakeExpensiveBoolOperationCallCount.Should().Be(2);
        
        //multiple new requests occur during the slide threshold
        //notice the bool values have FLIPPED for keys 1+2
        var call3 = _sut.GetLazySlidingCacheValue("Key1", new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call4 = _sut.GetLazySlidingCacheValue("Key1", new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call5 = _sut.GetLazySlidingCacheValue("Key2", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call6 = _sut.GetLazySlidingCacheValue("Key2", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        
        //for fun... lets pretend a request for a DIFFERENT key also occurs at the same time!
        var call7 = _sut.GetLazySlidingCacheValue("Key3", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));

        Task.WaitAll(call3, call4, call5, call6, call7);
        
        //some better way of asserting a separate thread has updated this value without hacky wait checks??
        var key1CacheValue = true;
        var key2CacheValue = false;
        while (key1CacheValue && !key2CacheValue)
        {
            await Task.Delay(5);
            _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key1"), out var key1Value);
            key1CacheValue = (bool)((CacheService.LazySlideCacheObject)key1Value)?.Value!;
            
            _memoryCache.TryGetValue(CacheService.LazySlideCacheKey("Key2"), out var key2Value);
            key2CacheValue = (bool)((CacheService.LazySlideCacheObject)key2Value)?.Value!;
        }

        //current old cache value still returned
        call3.Result.Should().BeTrue();
        call4.Result.Should().BeTrue();
        
        //current old cache value still returned
        call5.Result.Should().BeFalse();
        call6.Result.Should().BeFalse();
        
        //new value returned
        call7.Result.Should().BeTrue();

        //only 5/7 of the requests triggered the expensive operation
        //the initial 2 PLUS an extra slide each for Keys1+2, PLUS a new request for key 3.
        FakeExpensiveBoolOperationCallCount.Should().Be(5);
        
        //the next time keys 1/2 requested, it should now be the new updated FLIPPED value
        //from the previous request slide value
        var call8 = _sut.GetLazySlidingCacheValue("Key1", new LazySlideCacheOptions<bool>(FakeExpensiveTrueOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        var call9 = _sut.GetLazySlidingCacheValue("Key2", new LazySlideCacheOptions<bool>(FakeExpensiveFalseOperation,
            DefaultSlideThreshold, DefaultCacheExpiration));
        
        Task.WhenAll(call8, call9);
        
        call8.Result.Should().BeFalse();
        call9.Result.Should().BeTrue();
        
        //no more operations triggered, the current cache value returned and no slide occurred
        FakeExpensiveBoolOperationCallCount.Should().Be(5);
    }
}
