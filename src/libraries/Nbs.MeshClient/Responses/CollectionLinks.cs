using System.Web;

namespace Nbs.MeshClient.Responses;

public record CollectionLinks
{
    public string? Self { get; set; }
    public string? Next { get; set; }

    public string? GetNextContinueFrom()
    {
        if (string.IsNullOrEmpty(Next))
            return null;

        if (!Next.Contains('?'))
            return null;

        var queryString = Next.Split('?')[1];
        var queryStringParameters = HttpUtility.ParseQueryString(queryString);
        var continueFrom = queryStringParameters.GetValues("continue_from");
        return continueFrom?.FirstOrDefault();
    }
}
