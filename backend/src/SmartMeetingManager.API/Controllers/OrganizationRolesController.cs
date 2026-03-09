using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using System.Security.Claims;

namespace SmartMeetingManager.API.Controllers;

[ApiController]
[Route("api/auth/me")]
[Authorize]
[Produces("application/json")]
public class OrganizationRolesController : ControllerBase
{
    private readonly IOrganizationRoleService _roleService;
    private readonly IOrganizationPermissionService _permissionService;
    private readonly ILogger<OrganizationRolesController> _logger;

    public OrganizationRolesController(
        IOrganizationRoleService roleService,
        IOrganizationPermissionService permissionService,
        ILogger<OrganizationRolesController> logger)
    {
        _roleService = roleService;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Lista cargos customizados da organização do usuário.
    /// </summary>
    [HttpGet("organization-roles")]
    [ProducesResponseType(typeof(List<OrganizationRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrganizationRoleDto>>> GetOrganizationRoles(CancellationToken cancellationToken)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == null)
            return BadRequest(new { error = "Nao foi possivel identificar sua organizacao." });

        var list = await _roleService.GetByOrganizationAsync(organizationId.Value, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Cria um cargo customizado. Requer ser Owner ou ter permissão ManageRoles.
    /// </summary>
    [HttpPost("organization-roles")]
    [ProducesResponseType(typeof(OrganizationRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrganizationRoleDto>> CreateOrganizationRole(
        [FromBody] CreateOrganizationRoleDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return Unauthorized();

        var canManage = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, Domain.OrganizationPermissions.ManageRoles, cancellationToken);
        if (!canManage)
            return StatusCode(403, new { error = "Voce nao tem permissao para criar cargos." });

        if (string.IsNullOrWhiteSpace(dto?.Name))
            return BadRequest(new { error = "Nome do cargo e obrigatorio." });

        var created = await _roleService.CreateAsync(organizationId.Value, dto!, cancellationToken);
        return Ok(created);
    }

    /// <summary>
    /// Atualiza um cargo customizado. Requer Owner ou ManageRoles.
    /// </summary>
    [HttpPut("organization-roles/{id:guid}")]
    [ProducesResponseType(typeof(OrganizationRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationRoleDto>> UpdateOrganizationRole(
        Guid id,
        [FromBody] UpdateOrganizationRoleDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return Unauthorized();

        var canManage = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, Domain.OrganizationPermissions.ManageRoles, cancellationToken);
        if (!canManage)
            return StatusCode(403, new { error = "Voce nao tem permissao para editar cargos." });

        if (string.IsNullOrWhiteSpace(dto?.Name))
            return BadRequest(new { error = "Nome do cargo e obrigatorio." });

        var updated = await _roleService.UpdateAsync(id, organizationId.Value, dto!, cancellationToken);
        if (updated == null)
            return NotFound(new { error = "Cargo nao encontrado." });
        return Ok(updated);
    }

    /// <summary>
    /// Remove um cargo customizado. Só é possível se nenhum membro estiver usando o cargo. Requer Owner ou ManageRoles.
    /// </summary>
    [HttpDelete("organization-roles/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteOrganizationRole(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (userId == null || organizationId == null)
            return Unauthorized();

        var canManage = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, Domain.OrganizationPermissions.ManageRoles, cancellationToken);
        if (!canManage)
            return StatusCode(403, new { error = "Voce nao tem permissao para excluir cargos." });

        var deleted = await _roleService.DeleteAsync(id, organizationId.Value, cancellationToken);
        if (!deleted)
            return BadRequest(new { error = "Cargo nao encontrado ou ainda esta em uso por membros." });
        return NoContent();
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
