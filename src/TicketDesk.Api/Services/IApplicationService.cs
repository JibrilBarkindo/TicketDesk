using TicketDesk.Api.Domain;
using TicketDesk.Api.Dtos;

namespace TicketDesk.Api.Services;

public interface IApplicationService
{
    Task<PagedResult<ApplicationResponseDto>> GetTicketsAsync(ApplicationQueryParameters query, CancellationToken ct = default);
    Task<ApplicationResponseDto?> GetTicketByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApplicationResponseDto> CreateTicketAsync(CreateApplicationDto dto, CancellationToken ct = default);
    Task<ApplicationResponseDto> UpdateTicketAsync(Guid id, UpdateApplicationDto dto, CancellationToken ct = default);
    Task<ApplicationResponseDto> ChangeStatusAsync(Guid id, ApplicationStatus newStatus, CancellationToken ct = default);
    Task DeleteTicketAsync(Guid id, CancellationToken ct = default);
}
