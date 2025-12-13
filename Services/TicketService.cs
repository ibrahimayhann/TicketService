using Microsoft.EntityFrameworkCore;
using TicketApi.Data;
using TicketApi.DTOs.Request;
using TicketApi.DTOs.Response;
using TicketApi.Entities;
using TicketApi.Enums;
using TicketApi.Exceptions;

namespace TicketApi.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TicketResponse>> GetAllAsync(TicketQueryRequest query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;
        if (pageSize > 100) pageSize = 100;

        var q = _db.Tickets.AsNoTracking().AsQueryable();

        // SEARCH (Title/Description/Assignee/Tags)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t =>
                t.Title.Contains(s) ||
                t.Description.Contains(s) ||
                (t.Assignee != null && t.Assignee.Contains(s)) ||
                (t.Tags != null && t.Tags.Contains(s)));
        }

        // FILTER
        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);

        if (query.Priority.HasValue)
            q = q.Where(t => t.Priority == query.Priority.Value);

        // SORT
        q = (query.Sort ?? "createdAtDesc").ToLowerInvariant() switch
        {
            "createdatasc" => q.OrderBy(t => t.CreatedAt),
            "updatedatdesc" => q.OrderByDescending(t => t.UpdatedAt),
            "updatedatasc" => q.OrderBy(t => t.UpdatedAt),
            _ => q.OrderByDescending(t => t.CreatedAt) // createdAtDesc default
        };

        var totalCount = await q.CountAsync();

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToResponse(t))
            .ToListAsync();

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
        var ticket = await _db.Tickets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            throw new NotFoundException($"Ticket not found. Id={id}");

        return ToResponse(ticket);
    }

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request)
    {
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
        await _db.SaveChangesAsync();

        return ToResponse(ticket);
    }

    public async Task UpdateAsync(int id, UpdateTicketRequest request)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            throw new NotFoundException($"Ticket not found. Id={id}");

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.Assignee = request.Assignee;
        ticket.Tags = request.Tags;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            throw new NotFoundException($"Ticket not found. Id={id}");

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
    }
    public async Task<List<TicketCommentResponse>> GetCommentsAsync(int ticketId)
    {
        // ticket var mı kontrol (404)
        var exists = await _db.Tickets.AsNoTracking().AnyAsync(x => x.Id == ticketId);
        if (!exists)
            throw new NotFoundException($"Ticket not found. Id={ticketId}");

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
        var ticketExists = await _db.Tickets.AsNoTracking().AnyAsync(x => x.Id == ticketId);
        if (!ticketExists)
            throw new NotFoundException($"Ticket not found. Id={ticketId}");

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
}
