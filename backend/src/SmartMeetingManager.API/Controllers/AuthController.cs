using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain;
using System.Security.Claims;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para autenticacao e gerenciamento de usuarios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOrganizationPermissionService _permissionService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;

    public AuthController(IUserService userService, IAuthService authService, IOrganizationPermissionService permissionService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _authService = authService;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuario
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage))
            });
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.RegisterAsync(dto, ipAddress);

        if (!result.Success)
            return BadRequest(result);

        SetRefreshTokenCookie(result.RefreshToken!);
        return Ok(result);
    }

    /// <summary>
    /// Faz login de um usuario
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage))
            });
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.LoginAsync(dto, ipAddress);

        if (!result.Success)
            return Unauthorized(result);

        SetRefreshTokenCookie(result.RefreshToken!);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza o token de acesso usando o refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto? dto = null)
    {
        var refreshToken = dto?.RefreshToken ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Refresh token nao fornecido"
            });
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

        if (!result.Success)
            return Unauthorized(result);

        SetRefreshTokenCookie(result.RefreshToken!);
        return Ok(result);
    }

    /// <summary>
    /// Faz logout (revoga o refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var ipAddress = GetIpAddress();
            await _authService.RevokeTokenAsync(refreshToken, ipAddress);
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logout realizado com sucesso" });
    }

    /// <summary>
    /// Obtem informacoes do usuario autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _authService.GetUserInfoAsync(userId.Value);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Lista organizacoes do usuario autenticado com seu role em cada uma
    /// </summary>
    [HttpGet("my-organizations")]
    [Authorize]
    [ProducesResponseType(typeof(List<MyOrganizationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<MyOrganizationItemDto>>> GetMyOrganizations(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var list = await _authService.GetMyOrganizationsAsync(userId.Value, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Lista membros da organizacao atual do usuario (para tela de Equipe)
    /// </summary>
    [HttpGet("me/organization-members")]
    [Authorize]
    [ProducesResponseType(typeof(List<OrganizationMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<OrganizationMemberDto>>> GetMyOrganizationMembers(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var list = await _authService.GetMyOrganizationMembersAsync(userId.Value, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Atualiza o perfil do usuario autenticado
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserInfoDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _authService.UpdateProfileAsync(userId.Value, dto);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Altera a senha do usuario autenticado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId.Value, dto);
        if (!result)
            return BadRequest(new { error = "Senha atual incorreta" });

        return Ok(new { message = "Senha alterada com sucesso" });
    }

    /// <summary>
    /// Solicita recuperacao de senha
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        return Ok(new { message = "Se o email existir, enviaremos instrucoes de recuperacao" });
    }

    /// <summary>
    /// Redefine a senha com token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        if (!result)
            return BadRequest(new { error = "Token invalido ou expirado" });

        return Ok(new { message = "Senha redefinida com sucesso" });
    }

    /// <summary>
    /// Convida um usuario para a organizacao. Requer permissao InviteMembers.
    /// </summary>
    [HttpPost("invite")]
    [Authorize]
    [ProducesResponseType(typeof(InviteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InviteResponseDto>> InviteUser([FromBody] InviteUserDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();

        var user = await _userService.GetByEmailAsync(dto.Email, cancellationToken);
        if (user == null)
            return BadRequest(new { error = "Usuario nao encontrado" });

        if (organizationId == null)
            return BadRequest(new { error = "Nao foi possivel identificar sua organizacao. Faca login novamente." });
        if (userId == null)
            return Unauthorized(new { error = "Sessao invalida. Faca login novamente." });

        var canInvite = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.InviteMembers, cancellationToken);
        if (!canInvite)
            return StatusCode(403, new { error = "Voce nao tem permissao para convidar membros." });

        var result = await _authService.InviteUserAsync(organizationId.Value, userId.Value, dto);
        if (result == null)
            return BadRequest(new { error = "Usuario ja faz parte da organizacao" });

        return Ok(result);
    }

    /// <summary>
    /// Lista convites pendentes da organizacao. Requer permissao InviteMembers.
    /// </summary>
    [HttpGet("invites")]
    [Authorize]
    [ProducesResponseType(typeof(List<InviteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<InviteResponseDto>>> GetPendingInvites(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (organizationId == null)
            return BadRequest(new { error = "Nao foi possivel identificar sua organizacao. Faca login novamente." });
        if (userId == null)
            return Unauthorized();

        var canInvite = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.InviteMembers, cancellationToken);
        if (!canInvite)
            return StatusCode(403, new { error = "Voce nao tem permissao para ver convites." });

        var invites = await _authService.GetPendingInvitesAsync(organizationId.Value);
        return Ok(invites);
    }

    /// <summary>
    /// Cancela um convite. Requer permissao CancelInvites.
    /// </summary>
    [HttpDelete("invites/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvite(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return Unauthorized();

        var canCancel = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.CancelInvites, cancellationToken);
        if (!canCancel)
            return StatusCode(403, new { error = "Voce nao tem permissao para cancelar convites." });

        var result = await _authService.CancelInviteAsync(id, organizationId.Value);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Remove um membro da organizacao. Requer permissao RemoveMembers. Nao e possivel remover o Owner.
    /// </summary>
    [HttpDelete("me/organization-members/{memberUserId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOrganizationMember(Guid memberUserId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return Unauthorized();

        var canRemove = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.RemoveMembers, cancellationToken);
        if (!canRemove)
            return StatusCode(403, new { error = "Voce nao tem permissao para remover membros." });

        var result = await _authService.RemoveMemberAsync(organizationId.Value, memberUserId);
        if (result == RemoveMemberResult.NotFound)
            return NotFound(new { error = "Membro nao encontrado." });
        if (result == RemoveMemberResult.CannotRemoveOwner)
            return BadRequest(new { error = "Nao e possivel remover o dono da organizacao." });

        return NoContent();
    }

    /// <summary>
    /// Aceita um convite para a equipe (usuario logado; email deve coincidir com o do convite)
    /// </summary>
    [HttpPost("accept-invite")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _authService.AcceptInviteAsync(userId.Value, dto.InviteCode.Trim(), dto.InvitePassword);
        return result switch
        {
            AcceptInviteResult.Success => Ok(new { message = "Convite aceito. Voce entrou na equipe." }),
            AcceptInviteResult.NotFoundOrExpired => BadRequest(new { error = "Convite nao encontrado ou expirado." }),
            AcceptInviteResult.WrongEmail => BadRequest(new { error = "Este convite e para outro email." }),
            AcceptInviteResult.WrongPassword => BadRequest(new { error = "Senha do convite incorreta." }),
            AcceptInviteResult.AlreadyMember => BadRequest(new { error = "Voce ja faz parte desta equipe." }),
            _ => BadRequest(new { error = "Nao foi possivel aceitar o convite." })
        };
    }

    // Helper methods
    private void SetRefreshTokenCookie(string token)
    {
        var isProduction = string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Production",
            StringComparison.OrdinalIgnoreCase);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            SameSite = SameSiteMode.Lax,
            Secure = isProduction
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string? GetIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
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
