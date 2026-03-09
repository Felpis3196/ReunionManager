namespace SmartMeetingManager.Domain.Entities;

public class TeamMessage
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
