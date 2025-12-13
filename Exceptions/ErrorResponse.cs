namespace TicketApi.Exceptions
{
    public sealed class ErrorResponse
    {
        public string Title { get; init; } = "An error occurred";
        public int Status { get; init; }
        public string? Detail { get; init; }
        public string TraceId { get; init; } = default!;
        public Dictionary<string, string[]>? Errors { get; init; }
    }
}
