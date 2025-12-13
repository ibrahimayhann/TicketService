using TicketApi.DTOs.Request;
using TicketApi.DTOs.Response;

namespace TicketApi.Services;

public interface ITicketService
{
    Task<PagedResult<TicketResponse>> GetAllAsync(TicketQueryRequest query);
    Task<TicketResponse> GetByIdAsync(int id);
    Task<TicketResponse> CreateAsync(CreateTicketRequest request);
    Task UpdateAsync(int id, UpdateTicketRequest request);
    Task DeleteAsync(int id);

    Task<List<TicketCommentResponse>> GetCommentsAsync(int ticketId);
    Task<TicketCommentResponse> AddCommentAsync(int ticketId, CreateTicketCommentRequest request);
    Task UpdateCommentAsync(int commentId, UpdateTicketCommentRequest request);
    Task DeleteCommentAsync(int commentId);

    Task<List<TicketStatusReportResponse>> GetTicketCountByStatusAsync();


}
