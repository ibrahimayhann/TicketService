namespace TicketApi.Exceptions
{
    
    public sealed class ErrorResponse
    {
        // init:
        // - Bu property'ler sadece object initializer ile set edilsin (immutable yaklaşım).
        public string Title { get; init; } = "An error occurred";
        public int Status { get; init; }
        public string? Detail { get; init; }

       
        //  Middleware payload oluştururken TraceId set ediyor.
        public string TraceId { get; init; } = default!;

        public Dictionary<string, string[]>? Errors { get; init; }
    }
}
