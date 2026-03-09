namespace SmartMeetingManager.Application.Interfaces;

public interface IOrganizationPermissionService
{
    /// <summary>
    /// Returns true if the user has the given permission in the organization (as Owner, system Admin/Member, or via custom role).
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, Guid organizationId, string permission, CancellationToken cancellationToken = default);
}
