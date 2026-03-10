using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IOrganizationPermissionService _permissionService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IOrganizationPermissionService permissionService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? ipAddress = null)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ja esta em uso"
                };
            }

            // First user in the system becomes site admin
            var isFirstUser = !await _context.Users.AnyAsync();

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.ToLower().Trim(),
                Name = dto.Name.Trim(),
                PasswordHash = HashPassword(dto.Password),
                IsActive = true,
                IsSiteAdmin = isFirstUser,
                EmailConfirmed = true, // For now, auto-confirm
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            Organization? organization = null;
            OrganizationRole role = OrganizationRole.Member;

            Invite? usedInvite = null;
            // Check for invite code
            if (!string.IsNullOrEmpty(dto.InviteCode))
            {
                var invite = await _context.Set<Invite>()
                    .Include(i => i.Organization)
                    .FirstOrDefaultAsync(i => 
                        i.InviteCode == dto.InviteCode && 
                        i.Email.ToLower() == dto.Email.ToLower() &&
                        i.Status == InviteStatus.Pending &&
                        i.ExpiresAt > DateTime.UtcNow);

                if (invite != null)
                {
                    usedInvite = invite;
                    if (!string.IsNullOrEmpty(invite.InvitePasswordHash))
                    {
                        if (string.IsNullOrEmpty(dto.InvitePassword))
                        {
                            return new AuthResponseDto
                            {
                                Success = false,
                                Message = "Este convite exige uma senha. Informe a senha do convite."
                            };
                        }
                        if (!VerifyPassword(dto.InvitePassword, invite.InvitePasswordHash))
                        {
                            return new AuthResponseDto
                            {
                                Success = false,
                                Message = "Senha do convite incorreta."
                            };
                        }
                    }
                    organization = invite.Organization;
                    if (invite.CustomRoleId.HasValue)
                    {
                        role = OrganizationRole.Member;
                    }
                    else
                    {
                        role = Enum.TryParse<OrganizationRole>(invite.Role, out var r) ? r : OrganizationRole.Member;
                    }
                    invite.Status = InviteStatus.Accepted;
                    invite.AcceptedAt = DateTime.UtcNow;
                }
            }

            // Create new organization only if no invite and org name provided (user becomes Owner)
            if (organization == null && !string.IsNullOrEmpty(dto.OrganizationName))
            {
                organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = dto.OrganizationName.Trim(),
                    InviteCode = GenerateInviteCode(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Organizations.Add(organization);
                role = OrganizationRole.Owner;
            }

            var inviteCustomRoleId = usedInvite?.CustomRoleId;

            // Add user to organization only when they have one (invite or created above)
            if (organization != null)
            {
                var membership = new OrganizationMember
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    UserId = user.Id,
                    Role = role,
                    CustomRoleId = inviteCustomRoleId,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Set<OrganizationMember>().Add(membership);
            }

            await _context.SaveChangesAsync();

            string roleDisplayName = role.ToString();
            if (inviteCustomRoleId.HasValue)
            {
                var customRole = await _context.OrganizationCustomRoles.FindAsync(inviteCustomRoleId.Value);
                if (customRole != null)
                    roleDisplayName = customRole.Name;
            }

            var canInvite = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.InviteMembers);
            var canManageRoles = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.ManageRoles);
            var canRemove = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.RemoveMembers);
            var canManageTasks = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.ManageTasks);
            var canAssignTasks = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.AssignTasks);
            var canCompleteAnyTask = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.CompleteAnyTask);
            var canViewAllTasks = organization != null && await _permissionService.HasPermissionAsync(user.Id, organization.Id, OrganizationPermissions.ViewAllTasks);

            // Generate tokens
            var accessToken = GenerateJwtToken(user, organization?.Id, roleDisplayName);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

            _logger.LogInformation("User registered: {Email}", user.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registro realizado com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                User = MapToUserInfo(user, organization, roleDisplayName, canInvite, canManageRoles, canRemove, canManageTasks, canAssignTasks, canCompleteAnyTask, canViewAllTasks)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", dto.Email);
            var detail = ex.Message;
            if (ex.InnerException != null)
                detail += " | " + ex.InnerException.Message;
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao registrar usuario",
                ErrorDetail = detail
            };
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.OrganizationMembers)
                    .ThenInclude(m => m.Organization)
                .Include(u => u.OrganizationMembers)
                    .ThenInclude(m => m.CustomRole)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha incorretos"
                };
            }

            // Check lockout
            if (user.LockoutEndAt.HasValue && user.LockoutEndAt > DateTime.UtcNow)
            {
                var remaining = (user.LockoutEndAt.Value - DateTime.UtcNow).Minutes;
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Conta bloqueada. Tente novamente em {remaining} minutos"
                };
            }

            // Verify password (users with placeholder hash from migration cannot login)
            if (!TryVerifyPassword(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                }
                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha incorretos"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Conta desativada"
                };
            }

            // Reset failed attempts
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Get organization membership (deterministic: Owner > Admin > Member, then most recent)
            var membership = user.OrganizationMembers
                .Where(m => m.IsActive)
                .OrderBy(m => m.Role)
                .ThenByDescending(m => m.JoinedAt)
                .FirstOrDefault();
            var organization = membership?.Organization;
            var roleDisplayName = membership?.CustomRole?.Name ?? membership?.Role.ToString() ?? "Member";
            var orgId = organization?.Id ?? Guid.Empty;
            var canInvite = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.InviteMembers);
            var canManageRoles = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.ManageRoles);
            var canRemove = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.RemoveMembers);
            var canManageTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.ManageTasks);
            var canAssignTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.AssignTasks);
            var canCompleteAnyTask = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.CompleteAnyTask);
            var canViewAllTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.ViewAllTasks);

            // Generate tokens
            var accessToken = GenerateJwtToken(user, organization?.Id, roleDisplayName);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

            _logger.LogInformation("User logged in: {Email}", user.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login realizado com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                User = MapToUserInfo(user, organization, roleDisplayName, canInvite, canManageRoles, canRemove, canManageTasks, canAssignTasks, canCompleteAnyTask, canViewAllTasks)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", dto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao fazer login"
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        try
        {
            var token = await _context.Set<RefreshToken>()
                .Include(t => t.User)
                    .ThenInclude(u => u.OrganizationMembers)
                        .ThenInclude(m => m.Organization)
                .Include(t => t.User)
                    .ThenInclude(u => u.OrganizationMembers)
                        .ThenInclude(m => m.CustomRole)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Token invalido ou expirado"
                };
            }

            var user = token.User;
            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Conta desativada"
                };
            }

            // Revoke old token
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = "Replaced by new token";

            // Generate new tokens (deterministic: Owner > Admin > Member, then most recent)
            var membership = user.OrganizationMembers
                .Where(m => m.IsActive)
                .OrderBy(m => m.Role)
                .ThenByDescending(m => m.JoinedAt)
                .FirstOrDefault();
            var organization = membership?.Organization;
            var roleDisplayName = membership?.CustomRole?.Name ?? membership?.Role.ToString() ?? "Member";
            var orgId = organization?.Id ?? Guid.Empty;
            var canInvite = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.InviteMembers);
            var canManageRoles = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.ManageRoles);
            var canRemove = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.RemoveMembers);
            var canManageTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.ManageTasks);
            var canAssignTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.AssignTasks);
            var canCompleteAnyTask = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.CompleteAnyTask);
            var canViewAllTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(user.Id, orgId, OrganizationPermissions.ViewAllTasks);

            var newAccessToken = GenerateJwtToken(user, organization?.Id, roleDisplayName);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);
            token.ReplacedByToken = newRefreshToken.Token;

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                User = MapToUserInfo(user, organization, roleDisplayName, canInvite, canManageRoles, canRemove, canManageTasks, canAssignTasks, canCompleteAnyTask, canViewAllTasks)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao atualizar token"
            };
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || !token.IsActive)
            return false;

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = "Revoked by user";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Revoke all refresh tokens
        var tokens = await _context.Set<RefreshToken>()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Password changed";
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (user == null)
            return true; // Don't reveal if user exists

        user.PasswordResetToken = GenerateSecureToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        // TODO: Send email with reset link
        _logger.LogInformation("Password reset requested for {Email}, token: {Token}", 
            user.Email, user.PasswordResetToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.PasswordResetToken == dto.Token && 
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return false;

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.OrganizationMembers)
                .ThenInclude(m => m.Organization)
            .Include(u => u.OrganizationMembers)
                .ThenInclude(m => m.CustomRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        var membership = user.OrganizationMembers.FirstOrDefault(m => m.IsActive);
        var roleDisplayName = membership?.CustomRole?.Name ?? membership?.Role.ToString() ?? "Member";
        var orgId = membership?.OrganizationId ?? Guid.Empty;
        var canInvite = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.InviteMembers);
        var canManageRoles = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.ManageRoles);
        var canRemove = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.RemoveMembers);
        var canManageTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.ManageTasks);
        var canAssignTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.AssignTasks);
        var canCompleteAnyTask = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.CompleteAnyTask);
        var canViewAllTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.ViewAllTasks);
        return MapToUserInfo(user, membership?.Organization, roleDisplayName, canInvite, canManageRoles, canRemove, canManageTasks, canAssignTasks, canCompleteAnyTask, canViewAllTasks);
    }

    public async Task<UserInfoDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _context.Users
            .Include(u => u.OrganizationMembers)
                .ThenInclude(m => m.Organization)
            .Include(u => u.OrganizationMembers)
                .ThenInclude(m => m.CustomRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            user.Name = dto.Name.Trim();

        if (dto.AvatarUrl != null)
            user.AvatarUrl = dto.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var membership = user.OrganizationMembers.FirstOrDefault(m => m.IsActive);
        var roleDisplayName = membership?.CustomRole?.Name ?? membership?.Role.ToString() ?? "Member";
        var orgId = membership?.OrganizationId ?? Guid.Empty;
        var canInvite = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.InviteMembers);
        var canManageRoles = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.ManageRoles);
        var canRemove = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.RemoveMembers);
        var canManageTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.ManageTasks);
        var canAssignTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.AssignTasks);
        var canCompleteAnyTask = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.CompleteAnyTask);
        var canViewAllTasks = orgId != Guid.Empty && await _permissionService.HasPermissionAsync(userId, orgId, OrganizationPermissions.ViewAllTasks);
        return MapToUserInfo(user, membership?.Organization, roleDisplayName, canInvite, canManageRoles, canRemove, canManageTasks, canAssignTasks, canCompleteAnyTask, canViewAllTasks);
    }

    public async Task<InviteResponseDto?> InviteUserAsync(Guid organizationId, Guid invitedById, InviteUserDto dto)
    {
        // Check if user already exists in organization (only active members block re-invite)
        var existingMember = await _context.Set<OrganizationMember>()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m =>
                m.OrganizationId == organizationId &&
                m.IsActive &&
                m.User.Email.ToLower() == dto.Email.ToLower());

        if (existingMember != null)
            return null;

        // Check for pending invite
        var existingInvite = await _context.Set<Invite>()
            .FirstOrDefaultAsync(i => 
                i.OrganizationId == organizationId && 
                i.Email.ToLower() == dto.Email.ToLower() &&
                i.Status == InviteStatus.Pending);

        if (existingInvite != null)
        {
            existingInvite.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return MapToInviteResponse(existingInvite);
        }

        Guid? customRoleId = null;
        var role = dto.Role ?? "Member";
        if (dto.CustomRoleId.HasValue)
        {
            var customRole = await _context.OrganizationCustomRoles
                .FirstOrDefaultAsync(r => r.Id == dto.CustomRoleId.Value && r.OrganizationId == organizationId);
            if (customRole != null)
            {
                customRoleId = customRole.Id;
                role = "Member";
            }
        }

        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = dto.Email.ToLower(),
            InviteCode = GenerateInviteCode(),
            Role = role,
            CustomRoleId = customRoleId,
            InvitePasswordHash = !string.IsNullOrWhiteSpace(dto.InvitePassword)
                ? HashPassword(dto.InvitePassword!)
                : null,
            Status = InviteStatus.Pending,
            InvitedById = invitedById,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.Set<Invite>().Add(invite);
        await _context.SaveChangesAsync();

        // TODO: Send invite email
        _logger.LogInformation("Invite sent to {Email} for organization {OrgId}", 
            invite.Email, organizationId);

        return MapToInviteResponse(invite);
    }

    public async Task<List<InviteResponseDto>> GetPendingInvitesAsync(Guid organizationId)
    {
        var invites = await _context.Set<Invite>()
            .Where(i => i.OrganizationId == organizationId && i.Status == InviteStatus.Pending)
            .ToListAsync();

        return invites.Select(MapToInviteResponse).ToList();
    }

    public async Task<List<InviteResponseDto>> GetPendingInvitesForUserAsync(string email)
    {
        var invites = await _context.Set<Invite>()
            .Include(i => i.Organization)
            .Where(i => i.Email.ToLower() == email.ToLower() && i.Status == InviteStatus.Pending && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invites.Select(MapToInviteResponse).ToList();
    }

    public async Task<bool> CancelInviteAsync(Guid inviteId, Guid organizationId)
    {
        var invite = await _context.Set<Invite>().FindAsync(inviteId);
        if (invite == null || invite.Status != InviteStatus.Pending || invite.OrganizationId != organizationId)
            return false;

        invite.Status = InviteStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AcceptInviteResult> AcceptInviteAsync(Guid userId, string inviteCode, string? invitePassword = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return AcceptInviteResult.WrongEmail;

        var invite = await _context.Set<Invite>()
            .Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.InviteCode == inviteCode && i.Status == InviteStatus.Pending && i.ExpiresAt > DateTime.UtcNow);

        if (invite == null)
            return AcceptInviteResult.NotFoundOrExpired;

        if (!string.Equals(invite.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            return AcceptInviteResult.WrongEmail;

        if (!string.IsNullOrEmpty(invite.InvitePasswordHash))
        {
            if (string.IsNullOrEmpty(invitePassword))
                return AcceptInviteResult.WrongPassword;
            if (!VerifyPassword(invitePassword, invite.InvitePasswordHash))
                return AcceptInviteResult.WrongPassword;
        }

        var existingMembership = await _context.Set<OrganizationMember>()
            .FirstOrDefaultAsync(m => m.OrganizationId == invite.OrganizationId && m.UserId == userId);
        if (existingMembership != null && existingMembership.IsActive)
            return AcceptInviteResult.AlreadyMember;

        var role = OrganizationRole.Member;
        if (!invite.CustomRoleId.HasValue)
            role = Enum.TryParse<OrganizationRole>(invite.Role, out var r) ? r : OrganizationRole.Member;

        if (existingMembership != null)
        {
            // Reativar membro removido anteriormente
            existingMembership.IsActive = true;
            existingMembership.Role = role;
            existingMembership.CustomRoleId = invite.CustomRoleId;
            existingMembership.JoinedAt = DateTime.UtcNow;
        }
        else
        {
            var membership = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = invite.OrganizationId,
                UserId = userId,
                Role = role,
                CustomRoleId = invite.CustomRoleId,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Set<OrganizationMember>().Add(membership);
        }
        invite.Status = InviteStatus.Accepted;
        invite.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} accepted invite {InviteId} and joined organization {OrgId}", userId, invite.Id, invite.OrganizationId);
        return AcceptInviteResult.Success;
    }

    public async Task<RemoveMemberResult> RemoveMemberAsync(Guid organizationId, Guid memberUserId)
    {
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == memberUserId && m.IsActive);
        if (member == null)
            return RemoveMemberResult.NotFound;
        if (member.Role == OrganizationRole.Owner)
            return RemoveMemberResult.CannotRemoveOwner;

        member.IsActive = false;
        member.CustomRoleId = null;
        await _context.SaveChangesAsync();
        return RemoveMemberResult.Success;
    }

    public async Task<List<MyOrganizationItemDto>> GetMyOrganizationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _context.Set<OrganizationMember>()
            .Where(m => m.UserId == userId && m.IsActive)
            .Include(m => m.Organization)
            .Include(m => m.CustomRole)
            .ToListAsync(cancellationToken);
        return list.Select(m => new MyOrganizationItemDto
        {
            Id = m.OrganizationId,
            Name = m.Organization!.Name,
            Role = m.CustomRole?.Name ?? m.Role.ToString()
        }).ToList();
    }

    public async Task<List<OrganizationMemberDto>> GetMyOrganizationMembersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myMembership = await _context.Set<OrganizationMember>()
            .Where(m => m.UserId == userId && m.IsActive)
            .Select(m => m.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken);
        if (myMembership == Guid.Empty)
            return new List<OrganizationMemberDto>();

        var members = await _context.Set<OrganizationMember>()
            .Where(m => m.OrganizationId == myMembership && m.IsActive)
            .Include(m => m.User)
            .Include(m => m.CustomRole)
            .ToListAsync(cancellationToken);
        return members.Select(m => new OrganizationMemberDto
        {
            UserId = m.UserId,
            Name = m.User!.Name,
            Email = m.User.Email,
            AvatarUrl = m.User.AvatarUrl,
            Role = m.CustomRole?.Name ?? m.Role.ToString()
        }).ToList();
    }

    // Helper methods
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    /// <summary>
    /// Verifies password without throwing when hash is invalid (e.g. placeholder from migration).
    /// </summary>
    private bool TryVerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash) || !hash.StartsWith("$2", StringComparison.Ordinal))
            return false;
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(User user, Guid? organizationId, string roleDisplayName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "SmartMeetingManager_DefaultSecretKey_ChangeInProduction_12345678"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, roleDisplayName),
            new("isSiteAdmin", user.IsSiteAdmin.ToString().ToLowerInvariant()),
            new("jti", Guid.NewGuid().ToString())
        };

        if (organizationId.HasValue)
            claims.Add(new Claim("organizationId", organizationId.Value.ToString()));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes());

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SmartMeetingManager",
            audience: _configuration["Jwt:Audience"] ?? "SmartMeetingManagerApp",
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private int GetAccessTokenExpiryMinutes()
    {
        return int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var minutes) ? minutes : 60;
    }

    private int GetRefreshTokenExpiryDays()
    {
        return int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 7;
    }

    private static UserInfoDto MapToUserInfo(User user, Organization? organization, string roleDisplayName,
        bool canInviteMembers = false, bool canManageRoles = false, bool canRemoveMembers = false,
        bool canManageTasks = false, bool canAssignTasks = false, bool canCompleteAnyTask = false, bool canViewAllTasks = false)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Role = roleDisplayName,
            IsSiteAdmin = user.IsSiteAdmin,
            OrganizationId = organization?.Id,
            OrganizationName = organization?.Name,
            CreatedAt = user.CreatedAt,
            CanInviteMembers = canInviteMembers,
            CanManageRoles = canManageRoles,
            CanRemoveMembers = canRemoveMembers,
            CanManageTasks = canManageTasks,
            CanAssignTasks = canAssignTasks,
            CanCompleteAnyTask = canCompleteAnyTask,
            CanViewAllTasks = canViewAllTasks
        };
    }

    private static InviteResponseDto MapToInviteResponse(Invite invite)
    {
        return new InviteResponseDto
        {
            Id = invite.Id,
            Email = invite.Email,
            InviteCode = invite.InviteCode,
            Status = invite.Status.ToString(),
            ExpiresAt = invite.ExpiresAt,
            HasPassword = !string.IsNullOrEmpty(invite.InvitePasswordHash)
        };
    }
}
