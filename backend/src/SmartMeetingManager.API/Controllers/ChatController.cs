using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.API.Hubs;
using System.Security.Claims;

namespace SmartMeetingManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ITeamChatService _chatService;
    private readonly IHubContext<TeamChatHub> _hubContext;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ITeamChatService chatService, IHubContext<TeamChatHub> hubContext, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Lista as ultimas mensagens do chat da organizacao do usuario.
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(typeof(IReadOnlyList<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetMessages(
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return BadRequest(new { error = "Usuario sem organizacao. Entre em uma equipe para acessar o chat." });

        limit = Math.Clamp(limit, 1, 100);
        var messages = await _chatService.GetMessagesAsync(organizationId.Value, userId.Value, limit, cancellationToken);
        return Ok(messages);
    }

    /// <summary>
    /// Envia uma mensagem no chat da organizacao. Dispara broadcast em tempo real via SignalR.
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        [FromBody] SendMessageDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return BadRequest(new { error = "Usuario sem organizacao. Entre em uma equipe para acessar o chat." });

        var msg = await _chatService.SendMessageAsync(organizationId.Value, userId.Value, dto.Text, cancellationToken);
        if (msg == null)
            return BadRequest(new { error = "Voce nao e membro ativo desta organizacao." });

        var groupName = "org_" + organizationId.Value;
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", msg, cancellationToken);

        return Ok(msg);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
