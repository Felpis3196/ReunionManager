using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain;
using SmartMeetingManager.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;
using TaskStatus = SmartMeetingManager.Domain.Entities.TaskStatus;
using TaskPriority = SmartMeetingManager.Domain.Entities.TaskPriority;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gestao de tarefas (escopo por organizacao e permissoes).
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrganizationPermissionService _permissionService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IUnitOfWork unitOfWork, IOrganizationPermissionService permissionService, ILogger<TasksController> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _logger = logger;
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

    /// <summary>
    /// Lista tarefas da organizacao (todas ou apenas as do usuario, conforme permissao ViewAllTasks).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll(
        [FromQuery] Guid? meetingId,
        [FromQuery] Guid? assignedToId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (!userId.HasValue || !organizationId.HasValue)
            return Unauthorized(new { error = "Usuario ou organizacao nao identificados" });

        TaskStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TaskStatus>(status, true, out var statusEnum))
            statusFilter = statusEnum;

        var viewAll = await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.ViewAllTasks)
            || await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.ManageTasks);
        var assignedFilter = viewAll ? assignedToId : userId;

        var tasks = await _unitOfWork.Tasks.GetByOrganizationIdAsync(organizationId.Value, assignedFilter, statusFilter, cancellationToken);
        var list = tasks.ToList();
        if (meetingId.HasValue)
            list = list.Where(t => t.MeetingId == meetingId.Value).ToList();

        return Ok(list.Select(MapToDto).ToList());
    }

    /// <summary>
    /// Busca uma tarefa por ID (mesma organizacao).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskDto>> GetById(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        var organizationId = GetOrganizationId();
        if (!organizationId.HasValue)
            return Unauthorized(new { error = "Organizacao nao identificada" });

        var task = await _unitOfWork.Tasks.GetByIdWithMeetingAsync(id, cancellationToken);
        if (task == null || task.Meeting == null || task.Meeting.OrganizationId != organizationId.Value)
            return NotFound(new { error = "Tarefa nao encontrada" });

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Cria uma nova tarefa (requer ManageTasks; reuniao e assignee na mesma organizacao).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody, Required] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (!userId.HasValue || !organizationId.HasValue)
            return Unauthorized(new { error = "Usuario ou organizacao nao identificados" });

        if (!await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.ManageTasks))
            return StatusCode(403, new { error = "Sem permissao para criar tarefas" });

        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting == null || meeting.OrganizationId != organizationId.Value)
            return BadRequest(new { error = "Reuniao nao encontrada ou fora da organizacao" });

        if (!await _unitOfWork.Organizations.IsUserMemberAsync(request.AssignedToId, organizationId.Value, cancellationToken))
            return BadRequest(new { error = "Usuario atribuido nao e membro da organizacao" });

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            ProjectId = request.ProjectId,
            AssignedToId = request.AssignedToId,
            Title = request.Title,
            Description = request.Description,
            Status = TaskStatus.Pending,
            Priority = Enum.TryParse<TaskPriority>(request.Priority, true, out var priority) ? priority : TaskPriority.Medium,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, MapToDto(task));
    }

    /// <summary>
    /// Atualiza uma tarefa (requer ManageTasks; alterar AssignedToId requer AssignTasks).
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskDto>> Update(
        [Required] Guid id,
        [FromBody, Required] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (!userId.HasValue || !organizationId.HasValue)
            return Unauthorized(new { error = "Usuario ou organizacao nao identificados" });

        if (!await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.ManageTasks))
            return StatusCode(403, new { error = "Sem permissao para editar tarefas" });

        var task = await _unitOfWork.Tasks.GetByIdWithMeetingAsync(id, cancellationToken);
        if (task == null || task.Meeting == null || task.Meeting.OrganizationId != organizationId.Value)
            return NotFound(new { error = "Tarefa nao encontrada" });

        if (request.AssignedToId.HasValue && request.AssignedToId.Value != task.AssignedToId)
        {
            if (!await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.AssignTasks))
                return StatusCode(403, new { error = "Sem permissao para atribuir tarefas" });
            if (!await _unitOfWork.Organizations.IsUserMemberAsync(request.AssignedToId.Value, organizationId.Value, cancellationToken))
                return BadRequest(new { error = "Usuario atribuido nao e membro da organizacao" });
        }

        if (!string.IsNullOrEmpty(request.Title))
            task.Title = request.Title;
        if (request.Description != null)
            task.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TaskStatus>(request.Status, true, out var status))
        {
            task.Status = status;
            if (status == TaskStatus.Completed)
                task.CompletedAt = DateTime.UtcNow;
        }
        if (!string.IsNullOrEmpty(request.Priority) && Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
            task.Priority = priority;
        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate.Value;
        if (request.AssignedToId.HasValue)
            task.AssignedToId = request.AssignedToId.Value;

        await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Marca tarefa como concluida (assignee ou permissao CompleteAnyTask/ManageTasks).
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskDto>> Complete(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (!userId.HasValue || !organizationId.HasValue)
            return Unauthorized(new { error = "Usuario ou organizacao nao identificados" });

        var task = await _unitOfWork.Tasks.GetByIdWithMeetingAsync(id, cancellationToken);
        if (task == null || task.Meeting == null || task.Meeting.OrganizationId != organizationId.Value)
            return NotFound(new { error = "Tarefa nao encontrada" });

        var canComplete = task.AssignedToId == userId.Value
            || await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.CompleteAnyTask)
            || await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.ManageTasks);
        if (!canComplete)
            return StatusCode(403, new { error = "Sem permissao para concluir esta tarefa" });

        task.Status = TaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Exclui uma tarefa (requer ManageTasks).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        if (!userId.HasValue || !organizationId.HasValue)
            return Unauthorized(new { error = "Usuario ou organizacao nao identificados" });

        if (!await _permissionService.HasPermissionAsync(userId.Value, organizationId.Value, OrganizationPermissions.ManageTasks))
            return StatusCode(403, new { error = "Sem permissao para excluir tarefas" });

        var task = await _unitOfWork.Tasks.GetByIdWithMeetingAsync(id, cancellationToken);
        if (task == null || task.Meeting == null || task.Meeting.OrganizationId != organizationId.Value)
            return NotFound(new { error = "Tarefa nao encontrada" });

        await _unitOfWork.Tasks.DeleteAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static TaskDto MapToDto(TaskEntity task)
    {
        return new TaskDto
        {
            Id = task.Id,
            MeetingId = task.MeetingId,
            ProjectId = task.ProjectId,
            AssignedToId = task.AssignedToId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt
        };
    }
}

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid AssignedToId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateTaskRequest(
    [Required] Guid MeetingId,
    Guid? ProjectId,
    [Required] Guid AssignedToId,
    [Required] string Title,
    string? Description,
    string Priority = "Medium",
    DateTime? DueDate = null
);

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    DateTime? DueDate,
    Guid? AssignedToId
);
