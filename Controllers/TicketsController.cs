using Microsoft.AspNetCore.Mvc;
using TicketApi.DTOs.Request;
using TicketApi.DTOs.Response;
using TicketApi.Services;

[ApiController] // Otomatik model binding + bazı varsayılan davranışları (örn: parametre bağlama) aktive eder
[Route("api/tickets")] // Bu controller altındaki tüm endpoint'lerin kök path'i: /api/tickets
public class TicketsController : ControllerBase
{
    // Service katmanını controller'a DI ile alıyoruz (iş kuralları controller'da değil service'de olmalı)
    private readonly ITicketService _service;

    public TicketsController(ITicketService service)
    {
        // Constructor injection: test edilebilirlik ve gevşek bağlılık sağlar
        _service = service;
    }

    // ------------------------------------------------------------
    // TICKETS
    // ------------------------------------------------------------

    // GET api/tickets?search=...&status=...&priority=...&sort=...&page=1&pageSize=10
    [HttpGet] // Listeleme endpoint'i
    [ProducesResponseType(StatusCodes.Status200OK)] // Başarılı olursa 200 döner
    public async Task<ActionResult<PagedResult<TicketResponse>>> GetAll([FromQuery] TicketQueryRequest query)
    {
        // [FromQuery]: QueryString üzerinden TicketQueryRequest içindeki alanlar doldurulur (arama/filtreleme/sıralama/sayfalama)
        var result = await _service.GetAllAsync(query);

        // Ok(...): 200 + response body
        return Ok(result);
    }

    // GET api/tickets/{id}
    [HttpGet("{id:int}")] // {id:int} -> route constraint: sadece int gelirse bu endpoint eşleşir
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Bulunamazsa 404 (muhtemelen service exception + middleware ile)
    public async Task<ActionResult<TicketResponse>> GetById(int id)
    {
        // Ticket bulunamazsa service katmanının exception fırlatması beklenir.
        var ticket = await _service.GetByIdAsync(id);

        return Ok(ticket);
    }

    // POST api/tickets
    [HttpPost] // Yeni kayıt oluşturma
    [ProducesResponseType(StatusCodes.Status201Created)] // Başarılı create -> 201
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Validasyon hatası vs. -> 400 (middleware/validation formatına bağlı)
    public async Task<ActionResult<TicketResponse>> Create([FromBody] CreateTicketRequest request)
    {
        // [FromBody]: JSON body -> CreateTicketRequest'e map edilir
        var created = await _service.CreateAsync(request);

        // CreatedAtAction:
        // - 201 döner
        // - Location header set eder (genelde /api/tickets/{id})
        // - response body olarak created objesini döner
        return CreatedAtAction(
            nameof(GetById),          // Hangi action'a referans verdiğimiz
            new { id = created.Id },  // Route parametresi (GetById'nin id'sini doldurur)
            created                  // Response body
        );
    }

    // PUT api/tickets/{id}
    [HttpPut("{id:int}")] // Güncelleme endpoint'i
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Başarılı update'te body dönmek istemiyorsan 204 idealdir
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest request)
    {
        // Güncelleme iş kuralları service'de
        await _service.UpdateAsync(id, request);

        // NoContent(): 204 döner, response body yok
        return NoContent();
    }

    // DELETE api/tickets/{id}
    [HttpDelete("{id:int}")] // Silme endpoint'i
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Silme işlemi service'e bırakılır (bulunamazsa exception fırlatabilir)
        await _service.DeleteAsync(id);

        return NoContent();
    }

    // ------------------------------------------------------------
    // COMMENTS (Ticket alt kaynağı)
    // ------------------------------------------------------------

    // GET api/tickets/{ticketId}/comments
    [HttpGet("{ticketId:int}/comments")] // Bir ticket'a ait yorumları getirir (nested resource)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TicketCommentResponse>>> GetComments(int ticketId)
    {
        // Ticket yoksa service 404'ü exception ile tetikleyebilir
        var comments = await _service.GetCommentsAsync(ticketId);

        return Ok(comments);
    }

    // POST api/tickets/{ticketId}/comments
    [HttpPost("{ticketId:int}/comments")] // Ticket'a yorum ekleme
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(
        int ticketId,
        [FromBody] CreateTicketCommentRequest request)
    {
        // Yorum oluşturma business logic service'de
        var created = await _service.AddCommentAsync(ticketId, request);

        // 201 döndürür (CreatedAtAction da kullanılabilirdi; yorum detay endpoint'i varsa daha REST olur)
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // PUT api/tickets/comments/{commentId}
    [HttpPut("comments/{commentId:int}")] // Yorum güncelleme (ticketId yerine commentId üzerinden gidiyor)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(
        int commentId,
        [FromBody] UpdateTicketCommentRequest request)
    {
        await _service.UpdateCommentAsync(commentId, request);

        return NoContent();
    }

    // DELETE api/tickets/comments/{commentId}
    [HttpDelete("comments/{commentId:int}")] // Yorum silme
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        await _service.DeleteCommentAsync(commentId);

        return NoContent();
    }

    // ------------------------------------------------------------
    // REPORTS
    // ------------------------------------------------------------

    // GET api/tickets/reports/status
    [HttpGet("reports/status")] // Ticket'ları status'a göre sayısal rapor olarak döndürür
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TicketStatusReportResponse>>> GetTicketCountByStatus()
    {
        // Gruplama/raporlama genelde repository/service katmanında yapılır
        var result = await _service.GetTicketCountByStatusAsync();

        return Ok(result);
    }

    // GET api/tickets/reports/priority
    [HttpGet("reports/priority")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TicketPriorityReportResponse>>> GetTicketCountByPriority()
    {
        var result = await _service.GetTicketCountByPriorityAsync();
        return Ok(result);
    }

}
