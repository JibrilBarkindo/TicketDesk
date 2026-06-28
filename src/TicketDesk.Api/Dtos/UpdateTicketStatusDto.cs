using System.ComponentModel.DataAnnotations;
using TicketDesk.Api.Domain;

namespace TicketDesk.Api.Dtos;

public class UpdateTicketStatusDto
{
    [Required]
    [EnumDataType(typeof(ApplicationStatus))]
    public ApplicationStatus Status { get; set; }
}
