namespace SmartMeetingManager.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; set; } = null!;
}

public class Invite
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    /// <summary>Optional password hash; when set, the invitee must provide this password when registering with the invite code.</summary>
    public string? InvitePasswordHash { get; set; }
    public string Role { get; set; } = "Member";
    /// <summary>When set, invite is for this custom role; on accept member gets CustomRoleId and Role=Member.</summary>
    public Guid? CustomRoleId { get; set; }
    public InviteStatus Status { get; set; } = InviteStatus.Pending;
    public Guid InvitedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public User InvitedBy { get; set; } = null!;
    public OrganizationCustomRole? CustomRole { get; set; }
}

public enum InviteStatus
{
    Pending,
    Accepted,
    Expired,
    Cancelled
}
