namespace Nbs.MeshClient.Responses
{
    public record ErrorResponse
    {
        public required IReadOnlyCollection<ErrorDetail> Detail { get; set; }

        public override string ToString()
        {
            return string.Join(", ", Detail);
        }
    }
}
