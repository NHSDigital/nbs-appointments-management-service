namespace Nhs.Appointments.Jobs.BlobAuditor.Sink.Config;

/// <summary>
/// Expressions defining which fields should be excluded.
/// 
/// Supported patterns:
/// 1. "property" - Removes a top-level property, e.g., "contactDetails"
/// 2. "nested.property" - Removes a nested property, e.g., "attendeeDetails.nhsNumber"
/// 3. "array[index]" - Removes a specific array element, e.g., "contactDetails[0]"
/// 4. "array[*].property" - Removes a property from all array elements, e.g., "contactDetails[*].value"
/// 
/// </summary>
public class SinkExclusion
{
    public string Source { get; init; } = string.Empty;
    public List<string> ExcludedPaths { get; init; } = [];
}
