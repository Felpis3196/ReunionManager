using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gestao de usuarios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os usuarios
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userDtos = await _userService.GetAllAsync(cancellationToken);
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Busca usuarios por termo
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        try
        {
            var filtered = await _userService.SearchAsync(q ?? string.Empty, 10, cancellationToken);
            return Ok(filtered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Busca um usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
                return NotFound(new { error = "Usuario nao encontrado" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo usuario
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.CreateAsync(dto, cancellationToken);
        if (user == null)
            return BadRequest(new { error = "Email ja cadastrado" });

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Atualiza um usuario
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(
        [Required] Guid id,
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, dto, cancellationToken);
        if (user == null)
            return NotFound(new { error = "Usuario nao encontrado" });

        return Ok(user);
    }

    /// <summary>
    /// Remove um usuario
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([Required] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _userService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = "Usuario nao encontrado" });

        return NoContent();
    }
}
