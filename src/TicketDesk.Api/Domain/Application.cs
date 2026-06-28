namespace TicketDesk.Api.Domain;

/// <summary>A support ticket tracked by the system.</summary>
public class Application
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public Interest Priority { get; set; } = Interest.Medium;
    public string? AssignedTo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
