namespace SmartMeetingManager.Domain.Entities;

/// <summary>
/// Custom role defined by the organization Owner. Permissions are stored as JSON array of permission keys.
/// </summary>
public class OrganizationCustomRole
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>JSON array of permission keys, e.g. ["InviteMembers","CancelInvites"]</summary>
    public string PermissionsJson { get; set; } = "[]";

    // Navigation
    public virtual Organization Organization { get; set; } = null!;
}
