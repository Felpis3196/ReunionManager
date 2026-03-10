using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Endpoints de overview global (multi-organizacao) exclusivos para Site Admin.
/// </summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
[Authorize]
public class AdminDashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        ILogger<AdminDashboardController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
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
    /// Resumo por organizacao (usuarios, reunioes, tarefas) para o Site Admin.
    /// </summary>
    [HttpGet("organizations/summary")]
    [ProducesResponseType(typeof(IEnumerable<AdminOrganizationSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AdminOrganizationSummaryDto>>> GetOrganizationsSummary(
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var isSiteAdmin = GetIsSiteAdmin();

            if (!userId.HasValue || !isSiteAdmin)
                return StatusCode(403, new { error = "Acesso restrito ao administrador da plataforma." });

            var organizations = await _unitOfWork.Organizations.GetAllAsync(cancellationToken);
            var allMeetings = await _unitOfWork.Meetings.GetAllAsync(cancellationToken);
            var allTasks = await _unitOfWork.Tasks.GetAllAsync(cancellationToken);

            var orgMembers = await _context.Set<OrganizationMember>()
                .Where(m => m.IsActive)
                .ToListAsync(cancellationToken);

            var result = new List<AdminOrganizationSummaryDto>();

            foreach (var org in organizations)
            {
                var meetings = allMeetings.Where(m => m.OrganizationId == org.Id).ToList();
                var meetingIds = meetings.Select(m => m.Id).ToHashSet();
                var tasks = allTasks.Where(t => meetingIds.Contains(t.MeetingId)).ToList();

                var totalTasks = tasks.Count;
                var completedTasks = tasks.Count(t => t.Status == SmartMeetingManager.Domain.Entities.TaskStatus.Completed);
                var completionRate = totalTasks > 0
                    ? Math.Round((double)completedTasks / totalTasks * 100, 1)
                    : 0;

                var memberCount = orgMembers.Count(m => m.OrganizationId == org.Id);

                result.Add(new AdminOrganizationSummaryDto
                {
                    OrganizationId = org.Id,
                    Name = org.Name,
                    TotalUsers = memberCount,
                    TotalMeetings = meetings.Count,
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    TaskCompletionRate = completionRate
                });
            }

            return Ok(result.OrderByDescending(o => o.TotalMeetings).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin organization summary");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Resumo global de tarefas por organizacao para o Site Admin.
    /// </summary>
    [HttpGet("tasks/summary")]
    [ProducesResponseType(typeof(IEnumerable<AdminTaskSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AdminTaskSummaryDto>>> GetTasksSummary(
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var isSiteAdmin = GetIsSiteAdmin();

            if (!userId.HasValue || !isSiteAdmin)
                return StatusCode(403, new { error = "Acesso restrito ao administrador da plataforma." });

            var organizations = await _unitOfWork.Organizations.GetAllAsync(cancellationToken);
            var allMeetings = await _unitOfWork.Meetings.GetAllAsync(cancellationToken);
            var allTasks = await _unitOfWork.Tasks.GetAllAsync(cancellationToken);

            var meetingsByOrg = allMeetings.GroupBy(m => m.OrganizationId)
                .ToDictionary(g => g.Key, g => g.Select(m => m.Id).ToHashSet());

            var result = new List<AdminTaskSummaryDto>();

            foreach (var org in organizations)
            {
                if (!meetingsByOrg.TryGetValue(org.Id, out var meetingIds) || meetingIds.Count == 0)
                {
                    result.Add(new AdminTaskSummaryDto
                    {
                        OrganizationId = org.Id,
                        OrganizationName = org.Name,
                        TotalTasks = 0,
                        CompletedTasks = 0,
                        TaskCompletionRate = 0
                    });
                    continue;
                }

                var tasks = allTasks.Where(t => meetingIds.Contains(t.MeetingId)).ToList();
                var totalTasks = tasks.Count;
                var completedTasks = tasks.Count(t => t.Status == SmartMeetingManager.Domain.Entities.TaskStatus.Completed);
                var completionRate = totalTasks > 0
                    ? Math.Round((double)completedTasks / totalTasks * 100, 1)
                    : 0;

                result.Add(new AdminTaskSummaryDto
                {
                    OrganizationId = org.Id,
                    OrganizationName = org.Name,
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    TaskCompletionRate = completionRate
                });
            }

            return Ok(result.OrderByDescending(r => r.TotalTasks).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin tasks summary");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class AdminOrganizationSummaryDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int TotalMeetings { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double TaskCompletionRate { get; set; }
}

public class AdminTaskSummaryDto
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double TaskCompletionRate { get; set; }
}

