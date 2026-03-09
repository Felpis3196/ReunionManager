using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gestao de pauta de reunioes
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId}/agenda")]
[Produces("application/json")]
public class AgendaController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AgendaController> _logger;

    public AgendaController(IUnitOfWork unitOfWork, ILogger<AgendaController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os itens de pauta de uma reuniao
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AgendaItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AgendaItemDto>>> GetAll(
        [Required] Guid meetingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var items = meeting.AgendaItems
                .OrderBy(a => a.Order)
                .Select(a => MapToDto(a))
                .ToList();

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agenda items for meeting {MeetingId}", meetingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Adiciona um item de pauta
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AgendaItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgendaItemDto>> Create(
        [Required] Guid meetingId,
        [FromBody, Required] CreateAgendaItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var maxOrder = meeting.AgendaItems.Any() 
                ? meeting.AgendaItems.Max(a => a.Order) 
                : 0;

            var item = new AgendaItem
            {
                Id = Guid.NewGuid(),
                MeetingId = meetingId,
                Order = request.Order ?? maxOrder + 1,
                Title = request.Title,
                Description = request.Description,
                EstimatedDuration = request.EstimatedMinutes.HasValue 
                    ? TimeSpan.FromMinutes(request.EstimatedMinutes.Value) 
                    : null,
                IsCompleted = false
            };

            meeting.AgendaItems.Add(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetAll), new { meetingId }, MapToDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agenda item for meeting {MeetingId}", meetingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um item de pauta
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AgendaItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendaItemDto>> Update(
        [Required] Guid meetingId,
        [Required] Guid id,
        [FromBody, Required] UpdateAgendaItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var item = meeting.AgendaItems.FirstOrDefault(a => a.Id == id);
            if (item == null)
                return NotFound(new { error = "Item de pauta nao encontrado" });

            if (!string.IsNullOrEmpty(request.Title))
                item.Title = request.Title;

            if (request.Description != null)
                item.Description = request.Description;

            if (request.Order.HasValue)
                item.Order = request.Order.Value;

            if (request.EstimatedMinutes.HasValue)
                item.EstimatedDuration = TimeSpan.FromMinutes(request.EstimatedMinutes.Value);

            if (request.IsCompleted.HasValue)
                item.IsCompleted = request.IsCompleted.Value;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agenda item {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marca item como discutido/concluido
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(AgendaItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendaItemDto>> MarkAsComplete(
        [Required] Guid meetingId,
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var item = meeting.AgendaItems.FirstOrDefault(a => a.Id == id);
            if (item == null)
                return NotFound(new { error = "Item de pauta nao encontrado" });

            item.IsCompleted = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing agenda item {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reordena os itens de pauta
    /// </summary>
    [HttpPost("reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        [Required] Guid meetingId,
        [FromBody, Required] List<ReorderItem> items,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            foreach (var reorder in items)
            {
                var item = meeting.AgendaItems.FirstOrDefault(a => a.Id == reorder.Id);
                if (item != null)
                    item.Order = reorder.Order;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Pauta reordenada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering agenda items for meeting {MeetingId}", meetingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um item de pauta
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [Required] Guid meetingId,
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var item = meeting.AgendaItems.FirstOrDefault(a => a.Id == id);
            if (item == null)
                return NotFound(new { error = "Item de pauta nao encontrado" });

            meeting.AgendaItems.Remove(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agenda item {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    private static AgendaItemDto MapToDto(AgendaItem item)
    {
        return new AgendaItemDto
        {
            Id = item.Id,
            MeetingId = item.MeetingId,
            Order = item.Order,
            Title = item.Title,
            Description = item.Description,
            EstimatedMinutes = item.EstimatedDuration?.TotalMinutes,
            IsCompleted = item.IsCompleted
        };
    }
}

public class AgendaItemDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? EstimatedMinutes { get; set; }
    public bool IsCompleted { get; set; }
}

public record CreateAgendaItemRequest(
    [Required] string Title,
    string? Description,
    int? Order,
    int? EstimatedMinutes
);

public record UpdateAgendaItemRequest(
    string? Title,
    string? Description,
    int? Order,
    int? EstimatedMinutes,
    bool? IsCompleted
);

public record ReorderItem(
    [Required] Guid Id,
    [Required] int Order
);
