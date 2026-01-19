using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.UseCases.Meetings;
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

    public MeetingsController(
        CreateMeetingCommand createMeetingCommand,
        GetMeetingByIdQuery getMeetingByIdQuery,
        GetAllMeetingsQuery getAllMeetingsQuery,
        UpdateMeetingCommand updateMeetingCommand,
        GenerateAgendaCommand generateAgendaCommand,
        ProcessTranscriptCommand processTranscriptCommand)
    {
        _createMeetingCommand = createMeetingCommand;
        _getMeetingByIdQuery = getMeetingByIdQuery;
        _getAllMeetingsQuery = getAllMeetingsQuery;
        _updateMeetingCommand = updateMeetingCommand;
        _generateAgendaCommand = generateAgendaCommand;
        _processTranscriptCommand = processTranscriptCommand;
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
        try
        {
            // TODO: Get organizerId from authenticated user
            // For now, use a default user ID from seed data
            // In production, extract from JWT token
            var organizerId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // From seed data - Admin User
            
            var meeting = await _createMeetingCommand.ExecuteAsync(dto, organizerId, cancellationToken);
            
            return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, meeting);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
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
    /// Request para processar transcrição
    /// </summary>
    public record ProcessTranscriptRequest(
        /// <summary>
        /// Conteúdo da transcrição da reunião
        /// </summary>
        [Required] string Transcript
    );
}