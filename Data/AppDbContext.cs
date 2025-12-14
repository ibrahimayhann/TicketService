using Microsoft.EntityFrameworkCore;
using TicketApi.Entities;

namespace TicketApi.Data
{
   
    public class AppDbContext : DbContext
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

       
        public DbSet<Ticket> Tickets => Set<Ticket>();

        // TicketComment tablosu/collection'ı.
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

               
                
                // - API/servisler arası saat kaymaları yaşanmasın
                e.Property(x => x.CreatedAt)
                    .HasColumnType("datetimeoffset(7)")
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .ValueGeneratedOnAdd();

                // UpdatedAt:
                // - Create anında null kalabilir, update olduğunda service set edebilir
                // - DB'de datetimeoffset olarak tutulur
                e.Property(x => x.UpdatedAt)
                    .HasColumnType("datetimeoffset(7)");
            });

            
            // TICKETCOMMENT TABLO KONFİGÜRASYONU
            modelBuilder.Entity<TicketComment>(e =>
            {
               
                e.Property(x => x.Author)
                    .IsRequired()
                    .HasMaxLength(80);

                
                e.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(500);

              
                e.HasIndex(x => x.TicketId);

                // İLİŞKİ (RELATIONSHIP) AYARI
               
                e.HasOne(x => x.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(x => x.TicketId)

                    // OnDelete(DeleteBehavior.Cascade):
                    // - Ticket silinirse, ona bağlı yorumlar da otomatik silinir.
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
