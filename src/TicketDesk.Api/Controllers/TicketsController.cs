using Microsoft.AspNetCore.Mvc;
using TicketDesk.Api.Dtos;
using TicketDesk.Api.Services;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApplicationsController  : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController (IApplicationService service) => _service = service;

    /// <summary>Returns a paged, optionally filtered list of tickets.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ApplicationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationResponseDto>>> GetTickets(
        [FromQuery] ApplicationQueryParameters query, CancellationToken ct)
        => Ok(await _service.GetTicketsAsync(query, ct));

    /// <summary>Returns a single ticket by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationResponseDto>> GetTicket(Guid id, CancellationToken ct)
    {
        var ticket = await _service.GetTicketByIdAsync(id, ct);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    /// <summary>Creates a new ticket. New tickets always start in the Open state.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplicationResponseDto>> CreateTicket(
        [FromBody] CreateApplicationDto dto, CancellationToken ct)
    {
        var created = await _service.CreateTicketAsync(dto, ct);
        return CreatedAtAction(nameof(GetTicket), new { id = created.Id }, created);
    }

    /// <summary>Updates the editable fields of an existing ticket.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationResponseDto>> UpdateTicket(
        Guid id, [FromBody] UpdateApplicationDto dto, CancellationToken ct)
        => Ok(await _service.UpdateTicketAsync(id, dto, ct));

    /// <summary>Transitions a ticket to a new status, enforcing the allowed state machine.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationResponseDto>> ChangeStatus(
        Guid id, [FromBody] UpdateApplicationStatusDto dto, CancellationToken ct)
        => Ok(await _service.ChangeStatusAsync(id, dto.Status, ct));

    /// <summary>Deletes a ticket.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket(Guid id, CancellationToken ct)
    {
        await _service.DeleteTicketAsync(id, ct);
        return NoContent();
    }
}
