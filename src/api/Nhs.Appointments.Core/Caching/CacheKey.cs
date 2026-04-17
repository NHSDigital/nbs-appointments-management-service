namespace Nhs.Appointments.Core.Caching;

public static class CacheKey
{
    public const string ClinicalService = "clinical-service";
    public const string RolesCacheKey = "roles";
    public const string NotificationConfiguration = "notification_configuration";
    public static string LazySlideCacheKey(string cacheKey) => $"LazySlide:{cacheKey}";
    public static string UserRolesCacheKey(string userId) => $"user_roles_{userId}";
    public static string GetCacheSiteServiceSupportDateRangeKey(string siteId, List<string> services, DateOnly from,
        DateOnly until) => $"site_{siteId}_supports_{string.Join('_', services)}_in_{from:yyyyMMdd}_{until:yyyyMMdd}";
}
