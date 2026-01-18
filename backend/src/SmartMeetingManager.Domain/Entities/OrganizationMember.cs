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
    public OrganizationRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
}