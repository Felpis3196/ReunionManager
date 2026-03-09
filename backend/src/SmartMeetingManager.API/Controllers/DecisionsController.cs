using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gestao de decisoes de reunioes
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId}/decisions")]
[Produces("application/json")]
public class DecisionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DecisionsController> _logger;

    public DecisionsController(IUnitOfWork unitOfWork, ILogger<DecisionsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as decisoes de uma reuniao
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DecisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DecisionDto>>> GetAll(
        [Required] Guid meetingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var decisions = meeting.Decisions.Select(d => MapToDto(d)).ToList();
            return Ok(decisions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting decisions for meeting {MeetingId}", meetingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Adiciona uma decisao a uma reuniao
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DecisionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DecisionDto>> Create(
        [Required] Guid meetingId,
        [FromBody, Required] CreateDecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var decision = new Decision
            {
                Id = Guid.NewGuid(),
                MeetingId = meetingId,
                Title = request.Title,
                Description = request.Description,
                MadeById = request.MadeById,
                MadeAt = DateTime.UtcNow,
                IsImplemented = false
            };

            meeting.Decisions.Add(decision);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetAll), new { meetingId }, MapToDto(decision));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating decision for meeting {MeetingId}", meetingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma decisao
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DecisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionDto>> Update(
        [Required] Guid meetingId,
        [Required] Guid id,
        [FromBody, Required] UpdateDecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var decision = meeting.Decisions.FirstOrDefault(d => d.Id == id);
            if (decision == null)
                return NotFound(new { error = "Decisao nao encontrada" });

            if (!string.IsNullOrEmpty(request.Title))
                decision.Title = request.Title;

            if (request.Description != null)
                decision.Description = request.Description;

            if (request.IsImplemented.HasValue)
                decision.IsImplemented = request.IsImplemented.Value;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(decision));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating decision {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marca uma decisao como implementada
    /// </summary>
    [HttpPost("{id}/implement")]
    [ProducesResponseType(typeof(DecisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionDto>> MarkAsImplemented(
        [Required] Guid meetingId,
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var decision = meeting.Decisions.FirstOrDefault(d => d.Id == id);
            if (decision == null)
                return NotFound(new { error = "Decisao nao encontrada" });

            decision.IsImplemented = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(decision));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking decision {Id} as implemented", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove uma decisao
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

            var decision = meeting.Decisions.FirstOrDefault(d => d.Id == id);
            if (decision == null)
                return NotFound(new { error = "Decisao nao encontrada" });

            meeting.Decisions.Remove(decision);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting decision {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    private static DecisionDto MapToDto(Decision decision)
    {
        return new DecisionDto
        {
            Id = decision.Id,
            MeetingId = decision.MeetingId,
            Title = decision.Title,
            Description = decision.Description,
            MadeById = decision.MadeById,
            MadeAt = decision.MadeAt,
            IsImplemented = decision.IsImplemented
        };
    }
}

public class DecisionDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? MadeById { get; set; }
    public DateTime MadeAt { get; set; }
    public bool IsImplemented { get; set; }
}

public record CreateDecisionRequest(
    [Required] string Title,
    [Required] string Description,
    Guid? MadeById = null
);

public record UpdateDecisionRequest(
    string? Title,
    string? Description,
    bool? IsImplemented
);
