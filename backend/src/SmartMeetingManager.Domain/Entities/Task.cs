namespace SmartMeetingManager.Domain.Entities;

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class Task
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid AssignedToId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual User AssignedTo { get; set; } = null!;
}