namespace SmartMeetingManager.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}