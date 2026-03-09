using SmartMeetingManager.Application.DTOs;

namespace SmartMeetingManager.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? ipAddress = null);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<UserInfoDto?> GetUserInfoAsync(Guid userId);
    Task<UserInfoDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<InviteResponseDto?> InviteUserAsync(Guid organizationId, Guid invitedById, InviteUserDto dto);
    Task<List<InviteResponseDto>> GetPendingInvitesAsync(Guid organizationId);
    Task<List<InviteResponseDto>> GetPendingInvitesForUserAsync(string email);
    Task<bool> CancelInviteAsync(Guid inviteId, Guid organizationId);
    /// <summary>Accept an invite for the current user (email must match invite). Returns true if accepted.</summary>
    Task<AcceptInviteResult> AcceptInviteAsync(Guid userId, string inviteCode, string? invitePassword = null);
    Task<List<MyOrganizationItemDto>> GetMyOrganizationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<OrganizationMemberDto>> GetMyOrganizationMembersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RemoveMemberResult> RemoveMemberAsync(Guid organizationId, Guid memberUserId);
}

public enum AcceptInviteResult { Success, NotFoundOrExpired, WrongEmail, WrongPassword, AlreadyMember }

public enum RemoveMemberResult { Success, NotFound, CannotRemoveOwner }
