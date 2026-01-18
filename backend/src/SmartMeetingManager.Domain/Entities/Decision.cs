namespace SmartMeetingManager.Domain.Entities;

public class Decision
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime MadeAt { get; set; }
    public Guid? MadeById { get; set; }
    public bool IsImplemented { get; set; }
    public DateTime? ImplementedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual User? MadeBy { get; set; }
}