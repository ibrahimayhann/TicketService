namespace TicketApi.DTOs.Request;

public sealed class UpdateTicketCommentRequest
{
    public string Message { get; set; } = default!;
}
