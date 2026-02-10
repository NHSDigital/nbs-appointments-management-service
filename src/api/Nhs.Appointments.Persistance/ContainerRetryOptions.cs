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
    /// Each retry attempt awaits the initial value ms, with no increased backoff factor. <br/>
    /// i.e. +100ms, +100ms, +100ms, ...
    /// </summary>
    Linear = 0,
    /// <summary>
    /// Each retry attempt awaits the initial value ms, with an incremental doubling backoff factor. <br/>
    /// i.e. +100ms, +200ms, +400ms, ...
    /// </summary>
    GeometricDouble = 1,
    /// <summary>
    /// Each retry attempt awaits with e ^ (initial value ms), with an incremental exponential backoff factor. <br/>
    /// i.e. +150ms, +403ms, +1096ms, ... <br/>
    /// </summary>
    Exponential = 2,
}
