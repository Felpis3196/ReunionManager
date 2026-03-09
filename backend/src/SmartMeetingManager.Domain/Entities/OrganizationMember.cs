namespace SmartMeetingManager.Domain.Entities;

public enum OrganizationRole
{
    Owner,
    Admin,
    Member
}

public class OrganizationMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>When set, member has this custom role; permissions come from it. Otherwise permissions from Role enum.</summary>
    public Guid? CustomRoleId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual OrganizationCustomRole? CustomRole { get; set; }
}