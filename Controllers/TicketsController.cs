using Microsoft.AspNetCore.Mvc;
using TicketApi.DTOs.Request;
using TicketApi.DTOs.Response;
using TicketApi.Services;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketsController(ITicketService service)
    {
        _service = service;
    }

    // GET api/tickets?search=...&status=...&priority=...&sort=...&page=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketResponse>>> GetAll([FromQuery] TicketQueryRequest query)
    {
        var result = await _service.GetAllAsync(query);
        return Ok(result);
    }

    // GET api/tickets/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetById(int id)
    {
        var ticket = await _service.GetByIdAsync(id);
        return Ok(ticket);
    }

    // POST api/tickets
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponse>> Create([FromBody] CreateTicketRequest request)
    {
        var created = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }

    // PUT api/tickets/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    // DELETE api/tickets/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    // ----------------------------
    // COMMENTS
    // ----------------------------

    // GET api/tickets/{ticketId}/comments
    [HttpGet("{ticketId:int}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TicketCommentResponse>>> GetComments(int ticketId)
    {
        var comments = await _service.GetCommentsAsync(ticketId);
        return Ok(comments);
    }

    // POST api/tickets/{ticketId}/comments
    [HttpPost("{ticketId:int}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(
        int ticketId,
        [FromBody] CreateTicketCommentRequest request)
    {
        var created = await _service.AddCommentAsync(ticketId, request);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // PUT api/tickets/comments/{commentId}
    [HttpPut("comments/{commentId:int}")]
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
    [HttpDelete("comments/{commentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        await _service.DeleteCommentAsync(commentId);
        return NoContent();
    }

    // GET api/tickets/reports/status
    [HttpGet("reports/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TicketStatusReportResponse>>> GetTicketCountByStatus()
    {
        var result = await _service.GetTicketCountByStatusAsync();
        return Ok(result);
    }

}
