namespace TicketApi.Entities;

public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public string Author { get; set; } = default!;
    public string Message { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } 
}
