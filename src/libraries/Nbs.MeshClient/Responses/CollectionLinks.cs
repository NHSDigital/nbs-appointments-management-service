using System.Web;

namespace Nbs.MeshClient.Responses;

/// <summary>
/// Collection Links
/// </summary>
public record CollectionLinks
{
    /// <summary>
    /// Self
    /// </summary>
    public string? Self { get; set; }
    /// <summary>
    /// Next
    /// </summary>
    public string? Next { get; set; }

    /// <summary>
    /// Get Next Continue From
    /// </summary>
    public string? GetNextContinueFrom()
    {
        if (string.IsNullOrEmpty(Next))
        {
            return null;
        }

        if (!Next.Contains('?'))
        {
            return null;
        }

        var queryString = Next.Split('?')[1];
        var queryStringParameters = HttpUtility.ParseQueryString(queryString);
        var continueFrom = queryStringParameters.GetValues("continue_from");
        return continueFrom?.FirstOrDefault();
    }
}
