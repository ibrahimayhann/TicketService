using Microsoft.EntityFrameworkCore;
using TicketApi.Data;
using TicketApi.DTOs.Request;
using TicketApi.DTOs.Response;
using TicketApi.Entities;
using TicketApi.Enums;
using TicketApi.Exceptions;

namespace TicketApi.Services;

// TicketService:
// - Ticket ile ilgili tüm iş kuralları burada.
// - Controller sadece burayı çağırır, DB sorgusuna girmez.
// - NotFound gibi durumlarda exception fırlatıp middleware'in 404'e çevirmesini bekleriz.
public class TicketService : ITicketService
{
    // EF Core DbContext (SQL Server ile konuşur)
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db)
    {
        // DI ile AppDbContext alınır
        _db = db;
    }

    public async Task<PagedResult<TicketResponse>> GetAllAsync(TicketQueryRequest query)
    {
        // ------------------------------------------------------------
        // SAYFALAMA PARAMETRELERİNİ GÜVENLİ HALE GETİR
        // ------------------------------------------------------------
        // Page 0/negatif gelirse 1 yap (en az 1. sayfa)
        var page = query.Page < 1 ? 1 : query.Page;

        // PageSize 0/negatif gelirse 10 yap
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        // Çok büyük pageSize istemesin diye üst sınır koyuyoruz
        if (pageSize > 100) pageSize = 100;

        // ------------------------------------------------------------
        // TEMEL QUERY
        // ------------------------------------------------------------
        // AsNoTracking:
        // - Okuma amaçlı listelerde performans sağlar
        // - Change tracking yapmadığı için daha hızlıdır
        var q = _db.Tickets.AsNoTracking().AsQueryable();

        // ------------------------------------------------------------
        // SEARCH (ARAMA) -> EF.Functions.Like
        // ------------------------------------------------------------
        // Neden Like?
        // - Contains çoğu zaman LIKE '%...%' üretir ve veri büyüdükçe pahalı olabilir.
        // - Like kullandığımızda ne yaptığımız daha net olur.
        // Not:
        // - Like da yine '%...%' ile başlıyorsa index avantajı sınırlı olabilir,
        //   ama küçük/orta projelerde yeterli ve anlaşılır.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Trim: baştaki/sondaki boşlukları at
            // %...%: metnin içinde geçenleri yakala
            var s = $"%{query.Search.Trim()}%";

            q = q.Where(t =>
                EF.Functions.Like(t.Title, s) ||
                EF.Functions.Like(t.Description, s) ||
                // Assignee/Tags null olabilir, null iken Like çalıştırmayalım
                (t.Assignee != null && EF.Functions.Like(t.Assignee, s)) ||
                (t.Tags != null && EF.Functions.Like(t.Tags, s)));
        }

        // ------------------------------------------------------------
        // FILTER (FİLTRE)
        // ------------------------------------------------------------
        // Nullable filtreler:
        // - HasValue: kullanıcı gerçekten filtre göndermiş mi?
        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);

        if (query.Priority.HasValue)
            q = q.Where(t => t.Priority == query.Priority.Value);

        // ------------------------------------------------------------
        // SORT (SIRALAMA) -> BUG FIXED
        // ------------------------------------------------------------
        // Önceden createdAtDesc case'i yoktu; default'a düşüp desc veriyordu.
        // Bu "tesadüfen çalışıyordu" ama API'de kafa karıştırır.
        // Şimdi case'leri netleştirdik.
        q = (query.Sort ?? "createdAtDesc").ToLowerInvariant() switch
        {
            "createdatasc" => q.OrderBy(t => t.CreatedAt),
            "createdatdesc" => q.OrderByDescending(t => t.CreatedAt),

            "updatedatasc" => q.OrderBy(t => t.UpdatedAt),
            "updatedatdesc" => q.OrderByDescending(t => t.UpdatedAt),

            // Tanımsız bir sort gelirse güvenli default: CreatedAt desc
            _ => q.OrderByDescending(t => t.CreatedAt)
        };

        // ------------------------------------------------------------
        // TOPLAM KAYIT SAYISI (PAGING İÇİN)
        // ------------------------------------------------------------
        // Frontend genelde:
        // - toplam kaç kayıt var
        // - kaç sayfa çıkar
        // gibi bilgileri ister.
        var totalCount = await q.CountAsync();

        // ------------------------------------------------------------
        // SAYFALAMA UYGULA + DTO PROJEKSİYONU
        // ------------------------------------------------------------
        // Skip: kaç kayıt atla
        // Take: kaç kayıt al
        // Select(ToResponse): entity'yi dışarıya direkt açmak yerine DTO dön (daha profesyonel)
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToResponse(t))
            .ToListAsync();

        // ------------------------------------------------------------
        // PAGED RESULT DÖN
        // ------------------------------------------------------------
        return new PagedResult<TicketResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TicketResponse> GetByIdAsync(int id)
    {
        // AsNoTracking: sadece okunacak
        var ticket = await _db.Tickets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        // Ticket yoksa null -> 404 için exception fırlat
        if (ticket is null)
            throw new NotFoundException($"Ticket not found. Id={id}");

        return ToResponse(ticket);
    }

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request)
    {
        // Yeni kayıt oluştururken:
        // - Status gibi default değerleri server belirler (client'a bırakma)
        // - CreatedAt/UpdatedAt server saatine göre set edilir
        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Assignee = request.Assignee,
            Tags = request.Tags,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tickets.Add(ticket);

        // SaveChanges:
        // - Insert SQL çalışır
        // - Identity Id varsa burada ticket.Id set edilir
        await _db.SaveChangesAsync();

        return ToResponse(ticket);
    }

    public async Task UpdateAsync(int id, UpdateTicketRequest request)
    {
        // Update işlemi için entity tracking gerekir.
        // Bu yüzden AsNoTracking kullanmıyoruz.
        var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            throw new NotFoundException($"Ticket not found. Id={id}");

        // Request'ten gelen alanları güncelle
        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.Assignee = request.Assignee;
        ticket.Tags = request.Tags;

        // UpdatedAt server set eder
        ticket.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            throw new NotFoundException($"Ticket not found. Id={id}");

        _db.Tickets.Remove(ticket);

        // DbContext'te Cascade ayarlıysa:
        // - Ticket silinince bağlı TicketComment kayıtları da otomatik silinir.
        await _db.SaveChangesAsync();
    }

    public async Task<List<TicketCommentResponse>> GetCommentsAsync(int ticketId)
    {
        // ------------------------------------------------------------
        // 1) Ticket var mı? (404)
        // ------------------------------------------------------------
        // AnyAsync hızlıdır: sadece var/yok kontrolü
        var exists = await _db.Tickets.AsNoTracking().AnyAsync(x => x.Id == ticketId);
        if (!exists)
            throw new NotFoundException($"Ticket not found. Id={ticketId}");

        // ------------------------------------------------------------
        // 2) Yorumları getir
        // ------------------------------------------------------------
        // TicketId index'li olduğu için bu sorgu verimli olur.
        return await _db.TicketComments.AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new TicketCommentResponse
            {
                Id = c.Id,
                TicketId = c.TicketId,
                Author = c.Author,
                Message = c.Message,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<TicketCommentResponse> AddCommentAsync(int ticketId, CreateTicketCommentRequest request)
    {
        // Ticket var mı kontrol et
        var ticketExists = await _db.Tickets.AsNoTracking().AnyAsync(x => x.Id == ticketId);
        if (!ticketExists)
            throw new NotFoundException($"Ticket not found. Id={ticketId}");

        // Trim:
        // - boşlukla gelen girişleri temizler
        // Not: FluentValidation ile zaten boş/uzunluk kontrolü yapıyorsan bu ekstra koruma olur.
        var comment = new TicketComment
        {
            TicketId = ticketId,
            Author = request.Author.Trim(),
            Message = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        return new TicketCommentResponse
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            Author = comment.Author,
            Message = comment.Message,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task UpdateCommentAsync(int commentId, UpdateTicketCommentRequest request)
    {
        var comment = await _db.TicketComments.FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment is null)
            throw new NotFoundException($"Comment not found. Id={commentId}");

        comment.Message = request.Message.Trim();

        await _db.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int commentId)
    {
        var comment = await _db.TicketComments.FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment is null)
            throw new NotFoundException($"Comment not found. Id={commentId}");

        _db.TicketComments.Remove(comment);
        await _db.SaveChangesAsync();
    }

    // ------------------------------------------------------------
    // ENTITY -> DTO MAPPING
    // ------------------------------------------------------------
    // Entity'yi dışarıya direkt vermek yerine DTO ile döndürmek:
    // - gereksiz alanları saklar
    // - ileride entity değişse bile API sözleşmesi daha stabil kalır
    private static TicketResponse ToResponse(Ticket t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        Assignee = t.Assignee,
        Tags = t.Tags
    };

    public async Task<List<TicketStatusReportResponse>> GetTicketCountByStatusAsync()
    {
        // Status bazında sayım raporu:
        // DB tarafında GROUP BY ile hesaplamak en doğru ve performanslı yoldur.
        return await _db.Tickets
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new TicketStatusReportResponse
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    // SQL karşılığı:
    // SELECT Status, COUNT(*) FROM Tickets GROUP BY Status;
}
