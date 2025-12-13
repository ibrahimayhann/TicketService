using TicketApi.Enums;

namespace TicketApi.DTOs.Response;

public sealed class TicketPriorityReportResponse
{
    public TicketPriority Priority { get; set; }
    public int Count { get; set; }
}
