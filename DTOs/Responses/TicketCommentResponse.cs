namespace TicketApi.DTOs.Response;

public sealed class TicketCommentResponse
{
    public int Id { get; set; }
    public int TicketId { get; set; }

    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
