using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.Domain;
using TicketDesk.Api.Dtos;
using TicketDesk.Api.Mapping;

namespace TicketDesk.Api.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public TicketService(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    // Defines which target states are reachable from each current state.
    private static readonly IReadOnlyDictionary<TicketStatus, TicketStatus[]> AllowedTransitions =
        new Dictionary<TicketStatus, TicketStatus[]>
        {
            [TicketStatus.Open] = new[] { TicketStatus.InProgress, TicketStatus.Resolved, TicketStatus.Closed },
            [TicketStatus.InProgress] = new[] { TicketStatus.Open, TicketStatus.Resolved, TicketStatus.Closed },
            [TicketStatus.Resolved] = new[] { TicketStatus.InProgress, TicketStatus.Closed },
            [TicketStatus.Closed] = Array.Empty<TicketStatus>()
        };

    public async Task<PagedResult<TicketResponseDto>> GetTicketsAsync(
        TicketQueryParameters query, CancellationToken ct = default)
    {
        var tickets = _db.Tickets.AsNoTracking();

        if (query.Status is not null)
            tickets = tickets.Where(t => t.Status == query.Status);
        if (query.Priority is not null)
            tickets = tickets.Where(t => t.Priority == query.Priority);
        if (!string.IsNullOrWhiteSpace(query.AssignedTo))
            tickets = tickets.Where(t => t.AssignedTo == query.AssignedTo);

        var totalCount = await tickets.CountAsync(ct);

        var entities = await tickets
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<TicketResponseDto>
        {
            Items = entities.Select(t => t.ToResponseDto()).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TicketResponseDto?> GetTicketByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        return ticket?.ToResponseDto();
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, CancellationToken ct = default)
    {
        var now = _clock.GetUtcNow();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Priority = dto.Priority,
            Status = TicketStatus.Open,
            AssignedTo = string.IsNullOrWhiteSpace(dto.AssignedTo) ? null : dto.AssignedTo.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(ct);
        return ticket.ToResponseDto();
    }

    public async Task<TicketResponseDto> UpdateTicketAsync(Guid id, UpdateTicketDto dto, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Ticket {id} was not found.");

        ticket.Title = dto.Title.Trim();
        ticket.Description = dto.Description.Trim();
        ticket.Priority = dto.Priority;
        ticket.AssignedTo = string.IsNullOrWhiteSpace(dto.AssignedTo) ? null : dto.AssignedTo.Trim();
        ticket.UpdatedAt = _clock.GetUtcNow();

        await _db.SaveChangesAsync(ct);
        return ticket.ToResponseDto();
    }

    public async Task<TicketResponseDto> ChangeStatusAsync(Guid id, TicketStatus newStatus, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Ticket {id} was not found.");

        if (ticket.Status == newStatus)
            return ticket.ToResponseDto();

        if (!AllowedTransitions[ticket.Status].Contains(newStatus))
            throw new DomainValidationException(
                $"Cannot change status from '{ticket.Status}' to '{newStatus}'.");

        ticket.Status = newStatus;
        ticket.UpdatedAt = _clock.GetUtcNow();
        await _db.SaveChangesAsync(ct);
        return ticket.ToResponseDto();
    }

    public async Task DeleteTicketAsync(Guid id, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Ticket {id} was not found.");

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync(ct);
    }
}
