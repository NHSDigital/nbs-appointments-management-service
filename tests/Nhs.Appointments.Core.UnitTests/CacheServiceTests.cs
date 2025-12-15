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
    
    private LazySlideCacheOptions<bool> DefaultOptions(string key = "ACacheKey") => new(key, FakeExpensiveBoolOperation, TimeSpan.FromHours(10), TimeSpan.FromHours(60));

    private async Task<bool> FakeExpensiveBoolOperation()
    {
        FakeExpensiveBoolOperationCallCount++;
        
        _timeProvider.CreateTimer(_ =>
        {
            _tcs?.TrySetResult(true);
        }, null, TimeSpan.FromHours(30), Timeout.InfiniteTimeSpan);
        
        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return await _tcs.Task;
    }

    public CacheServiceTests()
    {
        _sut = new CacheService(_memoryCache, _timeProvider);
    }
    
    [Fact]
    public async Task FakeTimerSetup()
    {
        var cacheValue = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        Assert.False(cacheValue.IsCompleted);
        
        _memoryCache.TryGetValue("ACacheKey", out var keyValue1);
        keyValue1.Should().BeNull();
        
        _timeProvider.Advance(TimeSpan.FromHours(60));
        
        (await cacheValue).Should().BeTrue();
        Assert.True(cacheValue.IsCompleted);
        
        _memoryCache.TryGetValue("ACacheKey", out var keyValue2);
        keyValue2.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Standard_Caching_Behaviour()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        var call2 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        var call3 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        //long operation has performed
        _timeProvider.Advance(TimeSpan.FromHours(45));
        
        (await call1).Should().BeTrue();
        (await call2).Should().BeTrue();
        (await call3).Should().BeTrue();
        
        _memoryCache.TryGetValue("ACacheKey", out var keyValue2);
        keyValue2.Should().NotBeNull();
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
    }
    
    [Fact]
    public async Task MultipleCallsWithinTimeframe_DoNot_TriggerMultipleExpensiveOperations()
    {
        var call1 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        FakeExpensiveBoolOperationCallCount.Should().Be(1);
        
        //long operation has performed
        _timeProvider.Advance(TimeSpan.FromHours(40));
        
        (await call1).Should().BeTrue();
        _memoryCache.TryGetValue("ACacheKey", out var keyValue2);
        keyValue2.Should().NotBeNull();
        
        //No easy way to simulate timeProvider with memoryCacheOptions
        //Just simulate the cache expiring
        _memoryCache.Remove("ACacheKey");
        
        var call2 = _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        _sut.GetLazySlidingCacheValue(DefaultOptions());
        
        _timeProvider.Advance(TimeSpan.FromHours(40));
        
        (await call2).Should().BeTrue();
        Assert.True(call2.IsCompleted);
        
        FakeExpensiveBoolOperationCallCount.Should().Be(2);
    }
}
