using TicketApi.Entities;
using TicketApi.Enums;

namespace TicketApi.DTOs.Response;

public class TicketResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? Assignee { get; set; }
    public string? Tags { get; set; }
}
