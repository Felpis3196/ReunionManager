using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Services;

public class OrganizationPermissionService : IOrganizationPermissionService
{
    private readonly ApplicationDbContext _context;

    public OrganizationPermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid organizationId, string permission, CancellationToken cancellationToken = default)
    {
        if (!OrganizationPermissions.IsValid(permission))
            return false;

        var membership = await _context.OrganizationMembers
            .Include(m => m.CustomRole)
            .FirstOrDefaultAsync(m =>
                m.UserId == userId &&
                m.OrganizationId == organizationId &&
                m.IsActive, cancellationToken);

        if (membership == null)
            return false;

        if (membership.Role == OrganizationRole.Owner)
            return true;

        if (membership.CustomRoleId.HasValue && membership.CustomRole != null)
        {
            var permissions = ParsePermissions(membership.CustomRole.PermissionsJson);
            return permissions.Contains(permission);
        }

        if (membership.Role == OrganizationRole.Admin)
        {
            return permission == OrganizationPermissions.InviteMembers ||
                   permission == OrganizationPermissions.CancelInvites ||
                   permission == OrganizationPermissions.RemoveMembers ||
                   permission == OrganizationPermissions.ManageRoles ||
                   permission == OrganizationPermissions.ManageTasks ||
                   permission == OrganizationPermissions.AssignTasks ||
                   permission == OrganizationPermissions.CompleteAnyTask ||
                   permission == OrganizationPermissions.ViewAllTasks;
        }

        return false;
    }

    private static IReadOnlySet<string> ParsePermissions(string permissionsJson)
    {
        if (string.IsNullOrWhiteSpace(permissionsJson))
            return new HashSet<string>();

        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(permissionsJson);
            return list != null ? new HashSet<string>(list, StringComparer.OrdinalIgnoreCase) : new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }
}
