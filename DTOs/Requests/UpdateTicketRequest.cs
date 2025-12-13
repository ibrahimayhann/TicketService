using System.ComponentModel.DataAnnotations;
using TicketApi.Enums;

namespace TicketApi.DTOs.Request
{
    public class UpdateTicketRequest
    {
        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }

        public string? Assignee { get; set; }
        public string? Tags { get; set; }
    }
}
