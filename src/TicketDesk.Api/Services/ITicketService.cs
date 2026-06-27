using TicketDesk.Api.Domain;
using TicketDesk.Api.Dtos;

namespace TicketDesk.Api.Services;

public interface ITicketService
{
    Task<PagedResult<TicketResponseDto>> GetTicketsAsync(TicketQueryParameters query, CancellationToken ct = default);
    Task<TicketResponseDto?> GetTicketByIdAsync(Guid id, CancellationToken ct = default);
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, CancellationToken ct = default);
    Task<TicketResponseDto> UpdateTicketAsync(Guid id, UpdateTicketDto dto, CancellationToken ct = default);
    Task<TicketResponseDto> ChangeStatusAsync(Guid id, TicketStatus newStatus, CancellationToken ct = default);
    Task DeleteTicketAsync(Guid id, CancellationToken ct = default);
}
