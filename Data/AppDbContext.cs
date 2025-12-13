using Microsoft.EntityFrameworkCore;
using TicketApi.Entities;

namespace TicketApi.Data
{
    // DbContext: EF Core'un veritabanı ile konuştuğu ana sınıftır.
    // - Hangi tablolar var (DbSet)
    // - Bu tabloların kolon/ilişki/constraint ayarları nasıl (OnModelCreating)
    // gibi her şeyin merkezi burasıdır.
    public class AppDbContext : DbContext
    {
        // DbContextOptions<AppDbContext>:
        // - Connection string
        // - Provider (SqlServer, PostgreSQL vs.)
        // - Logging, LazyLoading gibi EF ayarları
        // Bu options Program.cs tarafında AddDbContext ile hazırlanır.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet<Ticket>:
        // - Ticket entity'si için bir tablo/collection temsilidir.
        // - EF Core, migration oluştururken bunu tablo olarak görür (genelde "Tickets").
        // - LINQ ile sorgu atarken burayı kullanırsın: _context.Tickets.Where(...)
        public DbSet<Ticket> Tickets => Set<Ticket>();

        // TicketComment tablosu/collection'ı.
        public DbSet<TicketComment> TicketComments => Set<TicketComment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Base çağrısı: EF Core'un varsayılan davranışlarını korumak için iyi pratik.
            // (Bazı provider/konvansiyon ayarları burada devreye girer)
            base.OnModelCreating(modelBuilder);

            // ------------------------------------------------------------
            // TICKET TABLO KONFİGÜRASYONU
            // ------------------------------------------------------------
            // modelBuilder.Entity<Ticket>(...):
            // Ticket entity'sinin veritabanındaki tablo şemasını özelleştiriyoruz.
            modelBuilder.Entity<Ticket>(e =>
            {
                // Title kolon ayarları:
                // IsRequired():
                // - Bu alanın NULL olmasına izin verme demektir.
                // - DB tarafında NOT NULL constraint oluşur.
                // - Ayrıca model validasyonu tarafında da (yaklaşımına göre) zorunlu kabul edilir.
                //
                // HasMaxLength(150):
                // - DB tarafında string uzunluğunu sınırlar (örn nvarchar(150))
                // - Hem veri tutarlılığı hem performans açısından iyi.
                // - Title gibi kısa alanlarda kesin önerilir.
                e.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                // Description kolon ayarları:
                // IsRequired():
                // - NULL olamaz.
                // HasMaxLength yok:
                // - Uzun metin olabilir (SQL Server'da nvarchar(max) gibi)
                // - Ticket açıklaması uzun olabileceği için mantıklı.
                e.Property(x => x.Description)
                    .IsRequired();

                // -----------------------------
                // INDEX NEDİR? NE İŞE YARAR?
                // -----------------------------
                // Index, veritabanının arama/filtreleme işlemlerini hızlandırması için oluşturulan "hızlandırıcı yapı"dır.
                //
                // Mantık:
                // - Index yoksa DB çoğunlukla tabloyu baştan sona tarar (Table Scan).
                // - Index varsa, DB o kolon üzerinden çok daha hızlı arama/filtreleme yapabilir.
                //
                // Ne zaman index koyulur?
                // - Sık filtrelenen/sıralanan kolonlara
                // - WHERE içinde çok kullanılan alanlara
                //
                // Dezavantajı var mı?
                // - Evet: Insert/Update/Delete işlemlerinde index de güncellendiği için yazma maliyeti artar.
                // - Ama "sık sorgulanan" kolonlarda performans kazancı genelde buna değer.
                //
                // Burada Status ve Priority için index koyman MANTIKLI:
                // Çünkü Ticket listelemede "status = Open" veya "priority = High" gibi filtreler çok olur.
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.Priority);

                // Not:
                // - Bu index'ler tek kolonlu (single-column) index.
                // - Eğer sıkça "status + priority" beraber filtreleniyorsa, composite index de düşünülebilir:
                //   e.HasIndex(x => new { x.Status, x.Priority });
                // Şu an için tek kolon index gayet yeterli ve anlaşılır.
            });

            // ------------------------------------------------------------
            // TICKETCOMMENT TABLO KONFİGÜRASYONU
            // ------------------------------------------------------------
            modelBuilder.Entity<TicketComment>(e =>
            {
                // Author kolon ayarları:
                // - Yorum yazan kişinin adı/e-maili vs olabilir.
                // - NULL olmasın + uzunluk kısıtlı.
                e.Property(x => x.Author)
                    .IsRequired()
                    .HasMaxLength(80);

                // Message kolon ayarları:
                // - Yorum metni, NULL olmasın, 500 karakteri geçmesin.
                // - Eğer daha uzun yorum gerekirse artırılır.
                e.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                // TicketId'ye index:
                // - Yorumları genelde "Bu ticket'ın yorumlarını getir" diye çekeriz:
                //   WHERE TicketId = 5
                // - Bu sorgu çok sık olacağı için TicketId index'i performans açısından ÇOK doğru.
                e.HasIndex(x => x.TicketId);

                // -----------------------------
                // İLİŞKİ (RELATIONSHIP) AYARI
                // -----------------------------
                // TicketComment -> Ticket ilişkisi:
                // - Bir comment (yorum) sadece 1 ticket'a bağlıdır (Many-to-One).
                // - Bir ticket'ın birden çok yorumu olabilir (One-to-Many).
                //
                // HasOne(x => x.Ticket):
                // - TicketComment entity'sinde bir navigation property var: Ticket
                //
                // WithMany(t => t.Comments):
                // - Ticket entity'sinde collection navigation var: Comments (List<TicketComment> gibi)
                //
                // HasForeignKey(x => x.TicketId):
                // - TicketComment tablosundaki TicketId kolonu foreign key'dir.
                // - Yani TicketComment, Ticket tablosuna TicketId üzerinden bağlıdır.
                e.HasOne(x => x.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(x => x.TicketId)

                    // OnDelete(DeleteBehavior.Cascade):
                    // - Cascade delete demek: Ticket silinirse, ona bağlı yorumlar da otomatik silinir.
                    // - Bu genelde "yorumlar ticket olmadan anlamsız" olduğu için doğru yaklaşımdır.
                    //
                    // Dikkat:
                    // - Ticket'ı silince yorumlar da gideceği için "geçmiş kayıt" tutmak istiyorsan cascade istemeyebilirsin.
                    // - Ama Ticket sistemi gibi bir senaryoda genelde cascade uygundur.
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
