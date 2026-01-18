namespace SmartMeetingManager.Domain.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}