using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using System.Security.Claims;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para notificacoes: convites, reunioes proximas, tarefas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IAuthService authService,
        IUnitOfWork unitOfWork,
        ILogger<NotificationsController> logger)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtem notificacoes do usuario: convites pendentes, reunioes proximas, tarefas a vencer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async System.Threading.Tasks.Task<ActionResult<NotificationsResponseDto>> GetNotifications(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var email = GetUserEmail();
        if (userId == null || string.IsNullOrEmpty(email))
            return Unauthorized();

        var now = DateTime.UtcNow;
        var inThirtyMinutes = now.AddMinutes(30);
        var inSevenDays = now.AddDays(7);

        var invites = new List<InviteResponseDto>();
        var upcomingMeetings = new List<NotificationMeetingDto>();
        var meetingsStartingSoon = new List<NotificationMeetingDto>();
        var tasksDueSoon = new List<NotificationTaskDto>();

        try
        {
            invites = await _authService.GetPendingInvitesForUserAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading invites for user {UserId}", userId);
        }

        try
        {
            var meetings = await _unitOfWork.Meetings.GetUpcomingByUserIdAsync(userId.Value, cancellationToken);
            var meetingsList = meetings.ToList();
            upcomingMeetings = meetingsList
                .Where(m => m.Status == MeetingStatus.Scheduled && m.ScheduledAt <= inSevenDays)
                .OrderBy(m => m.ScheduledAt)
                .Take(15)
                .Select(m => new NotificationMeetingDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    ScheduledAt = m.ScheduledAt,
                    Type = m.Type.ToString()
                })
                .ToList();
            meetingsStartingSoon = meetingsList
                .Where(m => m.Status == MeetingStatus.Scheduled && m.ScheduledAt >= now && m.ScheduledAt <= inThirtyMinutes)
                .OrderBy(m => m.ScheduledAt)
                .Select(m => new NotificationMeetingDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    ScheduledAt = m.ScheduledAt,
                    Type = m.Type.ToString()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading meetings for notifications, user {UserId}", userId);
        }

        try
        {
            var allTasks = await _unitOfWork.Tasks.GetByUserIdAsync(userId.Value, cancellationToken);
            tasksDueSoon = allTasks
                .Where(t => t.Status != Domain.Entities.TaskStatus.Completed && t.Status != Domain.Entities.TaskStatus.Cancelled
                    && t.DueDate.HasValue && t.DueDate.Value <= inSevenDays)
                .OrderBy(t => t.DueDate)
                .Take(15)
                .Select(t => new NotificationTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    MeetingId = t.MeetingId
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading tasks for notifications, user {UserId}", userId);
        }

        var response = new NotificationsResponseDto
        {
            Invites = invites,
            MeetingsStartingSoon = meetingsStartingSoon,
            UpcomingMeetings = upcomingMeetings,
            TasksDueSoon = tasksDueSoon,
            UnreadCount = invites.Count + meetingsStartingSoon.Count + upcomingMeetings.Count + tasksDueSoon.Count
        };

        return Ok(response);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    private string? GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }
}

public class NotificationsResponseDto
{
    public List<InviteResponseDto> Invites { get; set; } = new();
    public List<NotificationMeetingDto> MeetingsStartingSoon { get; set; } = new();
    public List<NotificationMeetingDto> UpcomingMeetings { get; set; } = new();
    public List<NotificationTaskDto> TasksDueSoon { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class NotificationMeetingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class NotificationTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid MeetingId { get; set; }
}
