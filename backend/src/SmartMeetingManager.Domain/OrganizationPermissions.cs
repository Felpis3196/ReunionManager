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
    public const string ManageTasks = "ManageTasks";
    public const string AssignTasks = "AssignTasks";
    public const string CompleteAnyTask = "CompleteAnyTask";
    public const string ViewAllTasks = "ViewAllTasks";

    public static readonly IReadOnlyList<string> All = new[]
    {
        InviteMembers,
        CancelInvites,
        RemoveMembers,
        ManageRoles,
        EditOrganization,
        ManageTasks,
        AssignTasks,
        CompleteAnyTask,
        ViewAllTasks
    };

    public static bool IsValid(string permission)
    {
        return All.Contains(permission);
    }
}
