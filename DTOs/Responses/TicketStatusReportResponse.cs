using TicketApi.Enums;

namespace TicketApi.DTOs.Response;

public class TicketStatusReportResponse
{
    public TicketStatus Status { get; set; }
    public int Count { get; set; }
}
