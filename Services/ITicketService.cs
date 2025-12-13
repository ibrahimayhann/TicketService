using TicketApi.DTOs.Request;
using TicketApi.DTOs.Response;

namespace TicketApi.Services;

// ITicketService:
// Bu interface, Ticket ile ilgili tüm iş kurallarını (business logic) dış dünyaya "sözleşme" olarak sunar.
// Controller doğrudan DbContext ile uğraşmaz; sadece bu interface üzerinden konuşur.
// Böylece:
// - Controller sade kalır
// - Test yazmak kolaylaşır (mock/fake ile ITicketService taklit edilebilir)
// - İş kuralları tek yerde toplanır (Service katmanı)
public interface ITicketService
{
    // ------------------------------------------------------------
    // TICKETS
    // ------------------------------------------------------------

    // Listeleme (sayfalı + filtreli + sıralı):
    // query içinde genelde şunlar olur:
    // - search (başlık/açıklama araması)
    // - status / priority filtreleri
    // - sort (örn: createdAt desc)
    // - page / pageSize
    //
    // Dönüş tipi PagedResult<TicketResponse>:
    // - sadece liste dönmek yerine toplam kayıt sayısı, toplam sayfa, mevcut sayfa gibi meta bilgilerle döner
    // - front-end için standart ve kullanışlıdır
    Task<PagedResult<TicketResponse>> GetAllAsync(TicketQueryRequest query);

    // Id ile tek bir ticket getirme:
    // - Ticket bulunamazsa ideal yaklaşım:
    //   "null dönmek" yerine "NotFound exception" fırlatmak.
    // - ExceptionMiddleware bu exception'ı 404'e çevirir.
    Task<TicketResponse> GetByIdAsync(int id);

    // Yeni ticket oluşturma:
    // request: client'tan gelen oluşturma verileri (Title, Description, Priority vs.)
    // dönüş: oluşturulan ticket'ın response modeli (Id dahil)
    Task<TicketResponse> CreateAsync(CreateTicketRequest request);

    // Ticket güncelleme:
    // - Body olarak UpdateTicketRequest gelir
    // - Dönüş tipi void gibi (Task) çünkü controller 204 döndürüyor (NoContent)
    // - Ticket bulunamazsa burada da exception fırlatılması beklenir
    Task UpdateAsync(int id, UpdateTicketRequest request);

    // Ticket silme:
    // - Ticket bulunamazsa exception fırlatılmalı
    // - Eğer DB'de Cascade Delete ayarlıysa (sen yorumlarda ayarladın),
    //   ticket silinince bağlı comment'lar otomatik silinir.
    Task DeleteAsync(int id);

    // ------------------------------------------------------------
    // COMMENTS (Ticket'ın alt kaynağı)
    // ------------------------------------------------------------

    // Bir ticket'a ait yorumları getirir:
    // - ticketId yoksa exception (404) mantıklıdır
    // - Dönüş: TicketCommentResponse listesi
    Task<List<TicketCommentResponse>> GetCommentsAsync(int ticketId);

    // Bir ticket'a yorum ekleme:
    // - ticketId route'tan gelir, request body sadece yorum bilgilerini taşır (Author, Message gibi)
    // - Ticket yoksa exception fırlatılır
    // - Dönüş: oluşturulan yorumun response modeli (Id, CreatedAt vs. içerebilir)
    Task<TicketCommentResponse> AddCommentAsync(int ticketId, CreateTicketCommentRequest request);

    // Yorum güncelleme:
    // - commentId üzerinden ilerleniyor
    // - comment bulunamazsa exception fırlatılır
    Task UpdateCommentAsync(int commentId, UpdateTicketCommentRequest request);

    // Yorum silme:
    // - comment bulunamazsa exception fırlatılır
    Task DeleteCommentAsync(int commentId);

    // ------------------------------------------------------------
    // REPORTS
    // ------------------------------------------------------------

    // Status'a göre ticket sayısı raporu:
    // Örn:
    // - Open: 12
    // - InProgress: 5
    // - Closed: 20
    //
    // Bu tarz raporlar genelde GROUP BY ile DB tarafında hesaplanır.
    Task<List<TicketStatusReportResponse>> GetTicketCountByStatusAsync();
    Task<List<TicketPriorityReportResponse>> GetTicketCountByPriorityAsync();

}
