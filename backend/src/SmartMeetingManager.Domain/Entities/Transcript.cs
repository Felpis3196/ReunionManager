namespace SmartMeetingManager.Domain.Entities;

public class Transcript
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? AudioFilePath { get; set; }
    public string? VideoFilePath { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
}