using System.ComponentModel.DataAnnotations;
using TicketDesk.Api.Domain;

namespace TicketDesk.Api.Dtos;

public class CreateApplicationDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(Interest))]
    public Interest Priority { get; set; } = Interest.Medium;

    [StringLength(100)]
    public string? AssignedTo { get; set; }
}
