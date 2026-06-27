using TicketDesk.Api.Domain;
using TicketDesk.Api.Dtos;

namespace TicketDesk.Api.Mapping;

public static class TicketMappingExtensions
{
    public static TicketResponseDto ToResponseDto(this Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status,
        Priority = ticket.Priority,
        AssignedTo = ticket.AssignedTo,
        CreatedAt = ticket.CreatedAt,
        UpdatedAt = ticket.UpdatedAt
    };
}
