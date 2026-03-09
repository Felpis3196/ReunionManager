namespace SmartMeetingManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSiteAdmin { get; set; } = false;
    public bool EmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Meeting> MeetingsAsOrganizer { get; set; } = new List<Meeting>();
    public virtual ICollection<MeetingParticipant> MeetingParticipants { get; set; } = new List<MeetingParticipant>();
    public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public virtual ICollection<OrganizationMember> OrganizationMembers { get; set; } = new List<OrganizationMember>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}