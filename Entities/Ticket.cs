using TicketApi.Enums;

namespace TicketApi.Entities    
{
    public class Ticket
    {
        public int Id { get; set; }

        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;

        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? Assignee { get; set; }
        public string? Tags { get; set; } // şimdilik CSV: "bug,ui,urgent"

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

    }
}
