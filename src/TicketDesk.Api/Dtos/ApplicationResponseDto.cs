using TicketDesk.Api.Domain;

namespace TicketDesk.Api.Dtos;

public class ApplicationResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public Interest Priority { get; set; }
    public string? AssignedTo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
