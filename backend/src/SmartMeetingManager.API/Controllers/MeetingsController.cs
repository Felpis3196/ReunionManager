using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.UseCases.Meetings;
using SmartMeetingManager.Application.Mappings;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gerenciamento de reuniões
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MeetingsController : ControllerBase
{
    private readonly CreateMeetingCommand _createMeetingCommand;
    private readonly GetMeetingByIdQuery _getMeetingByIdQuery;
    private readonly GetAllMeetingsQuery _getAllMeetingsQuery;
    private readonly UpdateMeetingCommand _updateMeetingCommand;
    private readonly GenerateAgendaCommand _generateAgendaCommand;
    private readonly ProcessTranscriptCommand _processTranscriptCommand;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MeetingsController> _logger;

    public MeetingsController(
        CreateMeetingCommand createMeetingCommand,
        GetMeetingByIdQuery getMeetingByIdQuery,
        GetAllMeetingsQuery getAllMeetingsQuery,
        UpdateMeetingCommand updateMeetingCommand,
        GenerateAgendaCommand generateAgendaCommand,
        ProcessTranscriptCommand processTranscriptCommand,
        IUnitOfWork unitOfWork,
        ILogger<MeetingsController> logger)
    {
        _createMeetingCommand = createMeetingCommand;
        _getMeetingByIdQuery = getMeetingByIdQuery;
        _getAllMeetingsQuery = getAllMeetingsQuery;
        _updateMeetingCommand = updateMeetingCommand;
        _generateAgendaCommand = generateAgendaCommand;
        _processTranscriptCommand = processTranscriptCommand;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as reuniões, opcionalmente filtradas por organização ou projeto
    /// </summary>
    /// <param name="organizationId">ID da organização para filtrar reuniões</param>
    /// <param name="projectId">ID do projeto para filtrar reuniões</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de reuniões</returns>
    /// <response code="200">Retorna a lista de reuniões</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MeetingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MeetingDto>>> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        var meetings = await _getAllMeetingsQuery.ExecuteAsync(organizationId, projectId, cancellationToken);
        return Ok(meetings);
    }

    /// <summary>
    /// Busca uma reunião específica por ID
    /// </summary>
    /// <param name="id">ID da reunião</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes da reunião</returns>
    /// <response code="200">Retorna os detalhes da reunião</response>
    /// <response code="404">Reunião não encontrada</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingDto>> GetById(
        [Required] Guid id, 
        CancellationToken cancellationToken)
    {
        var meeting = await _getMeetingByIdQuery.ExecuteAsync(id, cancellationToken);
        
        if (meeting == null)
            return NotFound(new { error = "Reunião não encontrada" });

        return Ok(meeting);
    }

    /// <summary>
    /// Cria uma nova reunião
    /// </summary>
    /// <param name="dto">Dados da reunião a ser criada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Reunião criada</returns>
    /// <response code="201">Reunião criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingDto>> Create(
        [FromBody, Required] CreateMeetingDto dto, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== INÍCIO CREATE MEETING ===");
        _logger.LogInformation($"Received DTO: OrganizationId={dto.OrganizationId}, Title={dto.Title}, Type={dto.Type}, ScheduledAt={dto.ScheduledAt}, Duration={dto.Duration}");
        
        try
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning($"ModelState invalid. Errors: {string.Join(", ", errors)}");
                
                return BadRequest(new 
                { 
                    error = "Dados inválidos",
                    details = errors,
                    message = string.Join("; ", errors)
                });
            }

            _logger.LogInformation("ModelState is valid. Proceeding to create meeting...");

            // TODO: Get organizerId from authenticated user
            // For now, use a default user ID from seed data
            // In production, extract from JWT token
            var organizerId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // From seed data - Admin User
            
            var meeting = await _createMeetingCommand.ExecuteAsync(dto, organizerId, cancellationToken);
            
            _logger.LogInformation($"Meeting created successfully with ID: {meeting.Id}");
            
            return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, meeting);
        }
        catch (ArgumentException ex)
        {
            // Validation errors from business logic
            _logger.LogWarning($"ArgumentException: {ex.Message}");
            
            return BadRequest(new 
            { 
                error = "Erro de validação",
                message = ex.Message,
                details = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            // Log the full error
            _logger.LogError(ex, "Error creating meeting");
            _logger.LogError($"Exception type: {ex.GetType().Name}");
            _logger.LogError($"Exception message: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            
            return BadRequest(new 
            { 
                error = "Erro ao criar reunião",
                message = ex.Message,
                details = new[] { ex.Message },
                type = ex.GetType().Name,
                innerException = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Atualiza uma reunião existente
    /// </summary>
    /// <param name="id">ID da reunião</param>
    /// <param name="dto">Dados atualizados da reunião</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Reunião atualizada</returns>
    /// <response code="200">Reunião atualizada com sucesso</response>
    /// <response code="404">Reunião não encontrada</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingDto>> Update(
        [Required] Guid id, 
        [FromBody, Required] UpdateMeetingDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _updateMeetingCommand.ExecuteAsync(id, dto, cancellationToken);
            
            if (meeting == null)
                return NotFound(new { error = "Reunião não encontrada" });

            return Ok(meeting);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gera uma pauta sugerida para a reunião usando IA
    /// </summary>
    /// <param name="id">ID da reunião</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Pauta sugerida gerada pela IA</returns>
    /// <response code="200">Pauta gerada com sucesso</response>
    /// <response code="404">Reunião não encontrada</response>
    [HttpPost("{id}/generate-agenda")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GenerateAgenda(
        [Required] Guid id, 
        CancellationToken cancellationToken)
    {
        try
        {
            var agenda = await _generateAgendaCommand.ExecuteAsync(id, cancellationToken);
            return Ok(new { agenda });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Processa uma transcrição de reunião e extrai decisões, ações e gera resumo
    /// </summary>
    /// <param name="id">ID da reunião</param>
    /// <param name="request">Dados da transcrição</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado do processamento</returns>
    /// <response code="200">Transcrição processada com sucesso</response>
    /// <response code="404">Reunião não encontrada</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost("{id}/process-transcript")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessTranscript(
        [Required] Guid id, 
        [FromBody, Required] ProcessTranscriptRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            await _processTranscriptCommand.ExecuteAsync(id, request.Transcript, cancellationToken);
            return Ok(new { message = "Transcrição processada com sucesso" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma reuniao
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetByIdAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            await _unitOfWork.Meetings.DeleteAsync(meeting, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meeting {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Inicia uma reuniao
    /// </summary>
    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingDto>> StartMeeting(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            if (meeting.Status != MeetingStatus.Scheduled)
                return BadRequest(new { error = "Apenas reunioes agendadas podem ser iniciadas" });

            meeting.Status = MeetingStatus.InProgress;
            meeting.StartedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MeetingMapper.ToDto(meeting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting meeting {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Finaliza uma reuniao
    /// </summary>
    [HttpPost("{id}/end")]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingDto>> EndMeeting(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            if (meeting.Status != MeetingStatus.InProgress)
                return BadRequest(new { error = "Apenas reunioes em andamento podem ser finalizadas" });

            meeting.Status = MeetingStatus.Completed;
            meeting.EndedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MeetingMapper.ToDto(meeting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending meeting {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancela uma reuniao
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingDto>> CancelMeeting(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            if (meeting.Status == MeetingStatus.Completed)
                return BadRequest(new { error = "Reunioes concluidas nao podem ser canceladas" });

            if (meeting.Status == MeetingStatus.Cancelled)
                return BadRequest(new { error = "Reuniao ja esta cancelada" });

            meeting.Status = MeetingStatus.Cancelled;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MeetingMapper.ToDto(meeting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling meeting {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Adiciona um participante a reuniao
    /// </summary>
    [HttpPost("{id}/participants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddParticipant(
        [Required] Guid id,
        [FromBody, Required] AddParticipantRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            if (meeting.Status != MeetingStatus.Scheduled)
                return BadRequest(new { error = "Participantes so podem ser adicionados a reunioes agendadas" });

            // Check if user is already a participant
            if (meeting.Participants.Any(p => p.UserId == request.UserId))
                return BadRequest(new { error = "Usuario ja e participante desta reuniao" });

            var participant = new MeetingParticipant
            {
                Id = Guid.NewGuid(),
                MeetingId = id,
                UserId = request.UserId,
                Status = ParticipantStatus.Invited,
                InvitedAt = DateTime.UtcNow
            };

            meeting.Participants.Add(participant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Participante adicionado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to meeting {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um participante da reuniao
    /// </summary>
    [HttpDelete("{id}/participants/{participantId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveParticipant(
        [Required] Guid id,
        [Required] Guid participantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var participant = meeting.Participants.FirstOrDefault(p => p.Id == participantId);
            if (participant == null)
                return NotFound(new { error = "Participante nao encontrado" });

            meeting.Participants.Remove(participant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from meeting {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza o status de um participante
    /// </summary>
    [HttpPatch("{id}/participants/{participantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateParticipantStatus(
        [Required] Guid id,
        [Required] Guid participantId,
        [FromBody, Required] UpdateParticipantStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(id, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var participant = meeting.Participants.FirstOrDefault(p => p.Id == participantId);
            if (participant == null)
                return NotFound(new { error = "Participante nao encontrado" });

            if (Enum.TryParse<ParticipantStatus>(request.Status, true, out var status))
            {
                participant.Status = status;
                
                if (status == ParticipantStatus.Attended)
                    participant.AttendedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Ok(new { message = "Status atualizado com sucesso" });
            }

            return BadRequest(new { error = "Status invalido" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant status");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Request para processar transcricao
    /// </summary>
    public record ProcessTranscriptRequest([Required] string Transcript);

    /// <summary>
    /// Request para adicionar participante
    /// </summary>
    public record AddParticipantRequest([Required] Guid UserId);

    /// <summary>
    /// Request para atualizar status do participante
    /// </summary>
    public record UpdateParticipantStatusRequest([Required] string Status);
}