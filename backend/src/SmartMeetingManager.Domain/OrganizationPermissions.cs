namespace SmartMeetingManager.Domain;

/// <summary>
/// Fixed set of organization-level permissions that can be assigned to custom roles.
/// </summary>
public static class OrganizationPermissions
{
    public const string InviteMembers = "InviteMembers";
    public const string CancelInvites = "CancelInvites";
    public const string RemoveMembers = "RemoveMembers";
    public const string ManageRoles = "ManageRoles";
    public const string EditOrganization = "EditOrganization";

    public static readonly IReadOnlyList<string> All = new[]
    {
        InviteMembers,
        CancelInvites,
        RemoveMembers,
        ManageRoles,
        EditOrganization
    };

    public static bool IsValid(string permission)
    {
        return All.Contains(permission);
    }
}
