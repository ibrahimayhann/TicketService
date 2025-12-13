namespace TicketApi.DTOs.Request;

public sealed class CreateTicketCommentRequest
{
    public string Author { get; set; } = default!;
    public string Message { get; set; } = default!;
}
