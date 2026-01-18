using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.UseCases.Meetings;

namespace SmartMeetingManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly CreateMeetingCommand _createMeetingCommand;
    private readonly GetMeetingByIdQuery _getMeetingByIdQuery;
    private readonly GenerateAgendaCommand _generateAgendaCommand;
    private readonly ProcessTranscriptCommand _processTranscriptCommand;

    public MeetingsController(
        CreateMeetingCommand createMeetingCommand,
        GetMeetingByIdQuery getMeetingByIdQuery,
        GenerateAgendaCommand generateAgendaCommand,
        ProcessTranscriptCommand processTranscriptCommand)
    {
        _createMeetingCommand = createMeetingCommand;
        _getMeetingByIdQuery = getMeetingByIdQuery;
        _generateAgendaCommand = generateAgendaCommand;
        _processTranscriptCommand = processTranscriptCommand;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MeetingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var meeting = await _getMeetingByIdQuery.ExecuteAsync(id, cancellationToken);
        
        if (meeting == null)
            return NotFound();

        return Ok(meeting);
    }

    [HttpPost]
    public async Task<ActionResult<MeetingDto>> Create([FromBody] CreateMeetingDto dto, CancellationToken cancellationToken)
    {
        // TODO: Get organizerId from authenticated user
        var organizerId = Guid.NewGuid(); // Placeholder
        
        var meeting = await _createMeetingCommand.ExecuteAsync(dto, organizerId, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, meeting);
    }

    [HttpPost("{id}/generate-agenda")]
    public async Task<ActionResult<string>> GenerateAgenda(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var agenda = await _generateAgendaCommand.ExecuteAsync(id, cancellationToken);
            return Ok(new { agenda });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/process-transcript")]
    public async Task<IActionResult> ProcessTranscript(Guid id, [FromBody] ProcessTranscriptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _processTranscriptCommand.ExecuteAsync(id, request.Transcript, cancellationToken);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    public record ProcessTranscriptRequest(string Transcript);
}