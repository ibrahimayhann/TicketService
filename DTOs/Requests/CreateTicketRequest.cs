using System.ComponentModel.DataAnnotations;
using TicketApi.Enums;

namespace TicketApi.DTOs.Request
{
    public class CreateTicketRequest
    {
        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public string? Assignee { get; set; }
        public string? Tags { get; set; }
    }
}
