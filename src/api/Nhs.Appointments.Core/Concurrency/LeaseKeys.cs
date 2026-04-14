namespace Nhs.Appointments.Core.Concurrency;

/// <summary>
/// This wrapper class is responsible for creating all lease keys
/// </summary>
public static class LeaseKeys
{
    /// <summary>
    /// This factory class is responsible for creating a lease key from the combination of the site and the passed date values.
    /// </summary>
    public static class SiteKeyFactory
    {
        /// <summary>
        /// This factory method creates a site lease key from the combination of the <paramref name="siteId"/> and <paramref name="date"/> values.
        /// </summary>
        /// <param name="siteId">The id of the site to be used to construct the key.</param>
        /// <param name="date">The date to be used to construct the key.</param>
        /// <returns>A key for the site lease lock.</returns>
        public static string Create(string siteId, DateOnly date)
        {
            ArgumentException.ThrowIfNullOrEmpty(siteId, nameof(siteId));

            return $"{siteId}_{date:yyyyMMdd}";
        }
    }
}
