namespace Nhs.Appointments.Persistance;

public class ContainerRetryOptions
{
    public List<ContainerRetryConfiguration> Configurations { get; set; }
}

public class ContainerRetryConfiguration
{
    /// <summary>
    /// The container that this pattern applies to
    /// </summary>
    public string ContainerName { get; set; }
    
    /// <summary>
    /// Backoff retry pattern
    /// </summary>
    public BackoffRetryType BackoffRetryType { get; set; }
    
    /// <summary>
    /// Initial value in milliseconds to be used by the BackoffRetryType
    /// </summary>
    public int InitialValueMs { get; set; }
    
    /// <summary>
    /// A total cutoff time that won't be exceeded for retry attempts for a single request
    /// </summary>
    public int CutoffRetryMs { get; set; }
}

public enum BackoffRetryType
{
    /// <summary>
    /// Each retry attempt awaits the retryAfter value
    /// </summary>
    CosmosDefault = 0,
    /// <summary>
    /// Each retry attempt awaits the initial value ms, with no increased backoff factor. <br/>
    /// i.e. +100ms, +100ms, +100ms, ...
    /// </summary>
    Linear = 1,
    /// <summary>
    /// Each retry attempt awaits the initial value ms, with an incremental doubling backoff factor. <br/>
    /// i.e. +100ms, +200ms, +400ms, ...
    /// </summary>
    GeometricDouble = 2,
    /// <summary>
    /// The first retry attempts with the provided initial value ms. Each subsequent retry uses a derived incremental exponential backoff value. <br/>
    /// i.e. +150ms, +407ms, +1108ms, ... <br/>
    /// [*Maths*: ln(150) = 5.01, so next retry value is 'e^(5.01+1) = 407', next retry value is 'e^(5.01+2) = 1108', ...]
    /// </summary>
    Exponential = 3,
}
