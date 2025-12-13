using Microsoft.EntityFrameworkCore;
using TicketApi.Entities;

namespace TicketApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketComment> TicketComments => Set<TicketComment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(e =>
            {
                e.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                e.Property(x => x.Description)
                    .IsRequired();

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.Priority);
            });

            modelBuilder.Entity<TicketComment>(e =>
            {
                e.Property(x => x.Author)
                    .IsRequired()
                    .HasMaxLength(80);

                e.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                e.HasIndex(x => x.TicketId);

                e.HasOne(x => x.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(x => x.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
