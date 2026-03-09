using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para dashboard e estatisticas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IUnitOfWork unitOfWork, ILogger<DashboardController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid? GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    private bool GetIsSiteAdmin()
    {
        var value = User.FindFirst("isSiteAdmin")?.Value;
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtem estatisticas gerais do dashboard
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardStats>> GetStats(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var isSiteAdmin = GetIsSiteAdmin();

            if (!userId.HasValue)
                return Forbid();

            // Dashboard geral: apenas SiteAdmin
            if (!organizationId.HasValue && !isSiteAdmin)
                return StatusCode(403, new { error = "Selecione uma organizacao para ver o dashboard ou acesse como administrador do site." });

            // Dashboard por org: usuario deve ser membro ou SiteAdmin
            if (organizationId.HasValue && !isSiteAdmin)
            {
                var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(userId.Value, organizationId.Value, cancellationToken);
                if (!isMember)
                    return Forbid();
            }

            var allMeetings = await _unitOfWork.Meetings.GetAllAsync(cancellationToken);
            var meetings = organizationId.HasValue 
                ? allMeetings.Where(m => m.OrganizationId == organizationId.Value).ToList()
                : allMeetings.ToList();

            var allTasksEnumerable = await _unitOfWork.Tasks.GetAllAsync(cancellationToken);
            var meetingIds = meetings.Select(m => m.Id).ToHashSet();
            var allTasks = organizationId.HasValue
                ? allTasksEnumerable.Where(t => meetingIds.Contains(t.MeetingId)).ToList()
                : allTasksEnumerable.ToList();

            var now = DateTime.UtcNow;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            // Calcular estatisticas
            var stats = new DashboardStats
            {
                TotalMeetings = meetings.Count,
                MeetingsThisMonth = meetings.Count(m => m.CreatedAt >= thisMonth),
                MeetingsLastMonth = meetings.Count(m => m.CreatedAt >= lastMonth && m.CreatedAt < thisMonth),
                
                ScheduledMeetings = meetings.Count(m => m.Status == MeetingStatus.Scheduled),
                CompletedMeetings = meetings.Count(m => m.Status == MeetingStatus.Completed),
                CancelledMeetings = meetings.Count(m => m.Status == MeetingStatus.Cancelled),
                InProgressMeetings = meetings.Count(m => m.Status == MeetingStatus.InProgress),

                TotalTasks = allTasks.Count(),
                PendingTasks = allTasks.Count(t => t.Status == Domain.Entities.TaskStatus.Pending),
                InProgressTasks = allTasks.Count(t => t.Status == Domain.Entities.TaskStatus.InProgress),
                CompletedTasks = allTasks.Count(t => t.Status == Domain.Entities.TaskStatus.Completed),

                TaskCompletionRate = allTasks.Any() 
                    ? Math.Round((double)allTasks.Count(t => t.Status == Domain.Entities.TaskStatus.Completed) / allTasks.Count() * 100, 1)
                    : 0,

                TotalMeetingHours = CalculateTotalMeetingHours(meetings.Where(m => m.Status == MeetingStatus.Completed)),
                AverageMeetingDuration = CalculateAverageDuration(meetings.Where(m => m.Status == MeetingStatus.Completed)),

                UpcomingMeetings = meetings
                    .Where(m => m.Status == MeetingStatus.Scheduled && m.ScheduledAt > now)
                    .OrderBy(m => m.ScheduledAt)
                    .Take(5)
                    .Select(m => new UpcomingMeetingSummary
                    {
                        Id = m.Id,
                        Title = m.Title,
                        ScheduledAt = m.ScheduledAt,
                        Type = m.Type.ToString(),
                        ParticipantCount = m.Participants.Count
                    })
                    .ToList(),

                RecentMeetings = meetings
                    .Where(m => m.Status == MeetingStatus.Completed)
                    .OrderByDescending(m => m.EndedAt)
                    .Take(5)
                    .Select(m => new RecentMeetingSummary
                    {
                        Id = m.Id,
                        Title = m.Title,
                        CompletedAt = m.EndedAt ?? m.ScheduledAt,
                        Duration = m.Duration.ToString(@"hh\:mm"),
                        DecisionCount = m.Decisions.Count,
                        TaskCount = m.Tasks.Count
                    })
                    .ToList(),

                MeetingsByType = meetings
                    .GroupBy(m => m.Type)
                    .Select(g => new MeetingTypeCount
                    {
                        Type = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList(),

                TasksByPriority = allTasks
                    .Where(t => t.Status != Domain.Entities.TaskStatus.Completed && t.Status != Domain.Entities.TaskStatus.Cancelled)
                    .GroupBy(t => t.Priority)
                    .Select(g => new TaskPriorityCount
                    {
                        Priority = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtem estatisticas de produtividade por periodo
    /// </summary>
    [HttpGet("productivity")]
    [ProducesResponseType(typeof(ProductivityStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductivityStats>> GetProductivityStats(
        [FromQuery] int days = 30,
        [FromQuery] Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetUserId();
            var isSiteAdmin = GetIsSiteAdmin();

            if (!userId.HasValue)
                return Forbid();

            if (!organizationId.HasValue && !isSiteAdmin)
                return StatusCode(403, new { error = "Selecione uma organizacao para ver o dashboard ou acesse como administrador do site." });

            if (organizationId.HasValue && !isSiteAdmin)
            {
                var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(userId.Value, organizationId.Value, cancellationToken);
                if (!isMember)
                    return Forbid();
            }

            var startDate = DateTime.UtcNow.AddDays(-days);
            var allMeetings = await _unitOfWork.Meetings.GetAllAsync(cancellationToken);
            var meetingsFiltered = organizationId.HasValue
                ? allMeetings.Where(m => m.OrganizationId == organizationId.Value && m.CreatedAt >= startDate).ToList()
                : allMeetings.Where(m => m.CreatedAt >= startDate).ToList();
            var meetings = meetingsFiltered;

            var completedMeetings = meetings.Where(m => m.Status == MeetingStatus.Completed).ToList();

            var stats = new ProductivityStats
            {
                Period = $"Ultimos {days} dias",
                TotalMeetings = meetings.Count,
                CompletedMeetings = completedMeetings.Count,
                TotalHoursInMeetings = CalculateTotalMeetingHours(completedMeetings),
                AverageDecisionsPerMeeting = completedMeetings.Any() 
                    ? Math.Round(completedMeetings.Average(m => m.Decisions.Count), 1) 
                    : 0,
                AverageTasksPerMeeting = completedMeetings.Any() 
                    ? Math.Round(completedMeetings.Average(m => m.Tasks.Count), 1) 
                    : 0,
                MeetingsWithDecisions = completedMeetings.Count(m => m.Decisions.Any()),
                MeetingsWithTasks = completedMeetings.Count(m => m.Tasks.Any()),
                
                DailyStats = Enumerable.Range(0, Math.Min(days, 14))
                    .Select(i => {
                        var date = DateTime.UtcNow.Date.AddDays(-i);
                        var dayMeetings = meetings.Where(m => m.ScheduledAt.Date == date).ToList();
                        return new DailyStat
                        {
                            Date = date.ToString("yyyy-MM-dd"),
                            MeetingCount = dayMeetings.Count,
                            TotalMinutes = (int)dayMeetings.Sum(m => m.Duration.TotalMinutes)
                        };
                    })
                    .Reverse()
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting productivity stats");
            return BadRequest(new { error = ex.Message });
        }
    }

    private double CalculateTotalMeetingHours(IEnumerable<Meeting> meetings)
    {
        return Math.Round(meetings.Sum(m => m.Duration.TotalHours), 1);
    }

    private string CalculateAverageDuration(IEnumerable<Meeting> meetings)
    {
        if (!meetings.Any()) return "00:00";
        var avgMinutes = meetings.Average(m => m.Duration.TotalMinutes);
        var ts = TimeSpan.FromMinutes(avgMinutes);
        return ts.ToString(@"hh\:mm");
    }
}

// DTOs para o Dashboard
public class DashboardStats
{
    public int TotalMeetings { get; set; }
    public int MeetingsThisMonth { get; set; }
    public int MeetingsLastMonth { get; set; }
    public int ScheduledMeetings { get; set; }
    public int CompletedMeetings { get; set; }
    public int CancelledMeetings { get; set; }
    public int InProgressMeetings { get; set; }
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double TaskCompletionRate { get; set; }
    public double TotalMeetingHours { get; set; }
    public string AverageMeetingDuration { get; set; } = "00:00";
    public List<UpcomingMeetingSummary> UpcomingMeetings { get; set; } = new();
    public List<RecentMeetingSummary> RecentMeetings { get; set; } = new();
    public List<MeetingTypeCount> MeetingsByType { get; set; } = new();
    public List<TaskPriorityCount> TasksByPriority { get; set; } = new();
}

public class UpcomingMeetingSummary
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
}

public class RecentMeetingSummary
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public string Duration { get; set; } = string.Empty;
    public int DecisionCount { get; set; }
    public int TaskCount { get; set; }
}

public class MeetingTypeCount
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TaskPriorityCount
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProductivityStats
{
    public string Period { get; set; } = string.Empty;
    public int TotalMeetings { get; set; }
    public int CompletedMeetings { get; set; }
    public double TotalHoursInMeetings { get; set; }
    public double AverageDecisionsPerMeeting { get; set; }
    public double AverageTasksPerMeeting { get; set; }
    public int MeetingsWithDecisions { get; set; }
    public int MeetingsWithTasks { get; set; }
    public List<DailyStat> DailyStats { get; set; } = new();
}

public class DailyStat
{
    public string Date { get; set; } = string.Empty;
    public int MeetingCount { get; set; }
    public int TotalMinutes { get; set; }
}
