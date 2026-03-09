using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;
using TaskStatus = SmartMeetingManager.Domain.Entities.TaskStatus;
using TaskPriority = SmartMeetingManager.Domain.Entities.TaskPriority;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gestao de tarefas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IUnitOfWork unitOfWork, ILogger<TasksController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as tarefas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll(
        [FromQuery] Guid? meetingId,
        [FromQuery] Guid? assignedToId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetAllAsync(cancellationToken);

            if (meetingId.HasValue)
                tasks = tasks.Where(t => t.MeetingId == meetingId.Value);

            if (assignedToId.HasValue)
                tasks = tasks.Where(t => t.AssignedToId == assignedToId.Value);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TaskStatus>(status, true, out var statusEnum))
                tasks = tasks.Where(t => t.Status == statusEnum);

            var taskDtos = tasks.Select(t => MapToDto(t)).ToList();
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Busca uma tarefa por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetById(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
                return NotFound(new { error = "Tarefa nao encontrada" });

            return Ok(MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma nova tarefa
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody, Required] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                MeetingId = request.MeetingId,
                ProjectId = request.ProjectId,
                AssignedToId = request.AssignedToId,
                Title = request.Title,
                Description = request.Description,
                Status = TaskStatus.Pending,
                Priority = Enum.TryParse<TaskPriority>(request.Priority, true, out var priority) 
                    ? priority 
                    : TaskPriority.Medium,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma tarefa
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Update(
        [Required] Guid id,
        [FromBody, Required] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
                return NotFound(new { error = "Tarefa nao encontrada" });

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marca tarefa como concluida
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Complete(
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
                return NotFound(new { error = "Tarefa nao encontrada" });

            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma tarefa
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
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
                return NotFound(new { error = "Tarefa nao encontrada" });

            await _unitOfWork.Tasks.DeleteAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
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
