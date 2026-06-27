using Microsoft.AspNetCore.Mvc;
using TicketDesk.Api.Dtos;
using TicketDesk.Api.Services;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketsController(ITicketService service) => _service = service;

    /// <summary>Returns a paged, optionally filtered list of tickets.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketResponseDto>>> GetTickets(
        [FromQuery] TicketQueryParameters query, CancellationToken ct)
        => Ok(await _service.GetTicketsAsync(query, ct));

    /// <summary>Returns a single ticket by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> GetTicket(Guid id, CancellationToken ct)
    {
        var ticket = await _service.GetTicketByIdAsync(id, ct);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    /// <summary>Creates a new ticket. New tickets always start in the Open state.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponseDto>> CreateTicket(
        [FromBody] CreateTicketDto dto, CancellationToken ct)
    {
        var created = await _service.CreateTicketAsync(dto, ct);
        return CreatedAtAction(nameof(GetTicket), new { id = created.Id }, created);
    }

    /// <summary>Updates the editable fields of an existing ticket.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicket(
        Guid id, [FromBody] UpdateTicketDto dto, CancellationToken ct)
        => Ok(await _service.UpdateTicketAsync(id, dto, ct));

    /// <summary>Transitions a ticket to a new status, enforcing the allowed state machine.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> ChangeStatus(
        Guid id, [FromBody] UpdateTicketStatusDto dto, CancellationToken ct)
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
