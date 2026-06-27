using System.ComponentModel.DataAnnotations;
using TicketDesk.Api.Domain;

namespace TicketDesk.Api.Dtos;

public class CreateTicketDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    [StringLength(100)]
    public string? AssignedTo { get; set; }
}
