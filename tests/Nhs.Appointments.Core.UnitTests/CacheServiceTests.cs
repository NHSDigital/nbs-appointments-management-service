using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;
using Nhs.Appointments.Core.Caching;

namespace Nhs.Appointments.Core.UnitTests;

public class CacheServiceTests
{
    private readonly CacheService _sut;
    
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
    
    private TaskCompletionSource<bool> _tcs;
    
    private int FakeExpensiveBoolOperationCallCount { get; set; }

    private TimeSpan ExpensiveOperationTimespan => TimeSpan.FromMinutes(30);
    
    private TimeSpan DefaultSlideThreshold => TimeSpan.FromHours(10);
    
    private TimeSpan DefaultCacheExpiration => TimeSpan.FromHours(60);
    
    private string DefaultCacheKey => "ACacheKey";
    
    private LazySlideCacheOptions<bool> DefaultOptions(string key = "ACacheKey") => new(key, FakeExpensiveTrueOperation, DefaultSlideThreshold, DefaultCacheExpiration);

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
        
        _timeProvider.CreateTimer(_ =>
        {
            _tcs?.TrySetResult(operationResult);
        }, null, ExpensiveOperationTimespan, Timeout.InfiniteTimeSpan);
        
        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return await _tcs.Task;
    }

    public CacheServiceTests()
    {
        _sut = new CacheService(_memoryCache, _timeProvider);
    }
    
    [Fact]
    public async Task CacheValuesSet()
    {
        var cacheValue = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        Assert.False(cacheValue.IsCompleted);
        
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue1);
        keyValue1.Should().BeNull();
        
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await cacheValue).Should().BeTrue();
        Assert.True(cacheValue.IsCompleted);
        
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue2);
        ((CacheService.LazySlideCacheObject)keyValue2)?.Value.Should().Be(true);
    }
    
    [Fact]
    public async Task Standard_Caching_Behaviour()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        var call2 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        var call3 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await call1).Should().BeTrue();
        (await call2).Should().BeTrue();
        (await call3).Should().BeTrue();
        
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
    }
    
    /// <summary>
    /// Update the cache value silently if it exists and the slide threshold has been passed
    /// </summary>
    [Fact]
    public async Task Standard_Lazy_Sliding_Behaviour()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await call1).Should().BeTrue();
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
        
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);
        
        //slide threshold has been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Add(TimeSpan.FromMinutes(1)));
        
        //the expensive operation now returns a DIFFERENT RESULT.
        //this cache value should be slid in the background for the next request, but this first usage returns the old value (lazy)
        var call2 = _sut.GetLazySlidingCacheValue(new LazySlideCacheOptions<bool>(DefaultCacheKey, FakeExpensiveFalseOperation, DefaultSlideThreshold, DefaultCacheExpiration));
        
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
            _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue2);
            slideCacheValue1 = (bool)((CacheService.LazySlideCacheObject)keyValue2)?.Value!;
        }
        
        Assert.False(slideCacheValue1);
        
        var call3 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
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
            _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue3);
            slideCacheValue2 = (bool)((keyValue3 as CacheService.LazySlideCacheObject)?.Value)!;
        }
        
        Assert.True(slideCacheValue2);
    }
    
    /// <summary>
    /// No sliding if below threshold
    /// </summary>
    [Fact]
    public async Task Standard_Lazy_Sliding_Behaviour_2()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await call1).Should().BeTrue();
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
        
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);
        
        //slide threshold has NOT YET been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Subtract(ExpensiveOperationTimespan).Subtract(TimeSpan.FromMinutes(10)));
        
        //second request WOULD have returned a new updated cache value, but we haven't hit the threshold yet
        var call2 = _sut.GetLazySlidingCacheValue(new LazySlideCacheOptions<bool>(DefaultCacheKey, FakeExpensiveFalseOperation, DefaultSlideThreshold, DefaultCacheExpiration));
        
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
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await call1).Should().BeTrue();
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);
        
        //No easy way to simulate timeProvider with memoryCacheOptions
        //Just simulate the cache expiring
        _memoryCache.Remove(DefaultCacheKey);
        
        var call2 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await call2).Should().BeTrue();
        Assert.True(call2.IsCompleted);
        
        FakeExpensiveBoolOperationCallCount.Should().Be(2);
    }
    
    /// <summary>
    /// If multiple requests are within the Slide threshold window, it does a SINGLE slide operation
    /// All the requests will return the outdated value until the cache is updated
    /// </summary>
    [Fact]
    public async Task MultipleCallsWithinTimeframe_DoNotTriggerMultipleExpensiveOperations_SlideThreshold()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        (await call1).Should().BeTrue();
        
        //slide threshold has been crossed
        _timeProvider.Advance(DefaultSlideThreshold.Add(TimeSpan.FromMinutes(1)));
        
        _memoryCache.TryGetValue(DefaultCacheKey, out var keyValue1);
        ((CacheService.LazySlideCacheObject)keyValue1)?.Value.Should().Be(true);
        
        //multiple requests occur during the slide threshold
        var call2 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        //only call2 should trigger a single update operation
        var call3 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        var call4 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        var call5 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        //expensive operation has performed
        _timeProvider.Advance(ExpensiveOperationTimespan.Add(TimeSpan.FromMinutes(1)));
        
        Task.WaitAll(call2, call3, call4, call5);
        
        (call2.Result).Should().BeTrue();
        (call3.Result).Should().BeTrue();
        (call4.Result).Should().BeTrue();
        (call5.Result).Should().BeTrue();
        
        FakeExpensiveBoolOperationCallCount.Should().Be(2);
    }
}
