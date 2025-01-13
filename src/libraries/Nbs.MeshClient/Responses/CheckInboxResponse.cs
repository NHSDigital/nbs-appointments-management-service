namespace Nbs.MeshClient.Responses
{
    public record CheckInboxResponse
    {
        public IReadOnlyCollection<string> Messages { get; set; } = Array.Empty<string>();
        public CollectionLinks? Links { get; set; }
    }
}
