using TicketApi.Enums;

namespace TicketApi.DTOs.Request;

public sealed class TicketQueryRequest
{
    public string? Search { get; set; }

    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }

    public string? Sort { get; set; } = "createdAtDesc";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
