using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Application.DTOs;

// Register DTOs
public class RegisterDto
{
    [Required(ErrorMessage = "Nome e obrigatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email e obrigatorio")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha e obrigatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no minimo 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmacao de senha e obrigatoria")]
    [Compare("Password", ErrorMessage = "Senhas nao conferem")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Optional: Create new organization or join existing
    public string? OrganizationName { get; set; }
    public string? InviteCode { get; set; }
    /// <summary>Required when registering with an invite that has a password.</summary>
    public string? InvitePassword { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "Email e obrigatorio")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha e obrigatoria")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Senha atual e obrigatoria")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha e obrigatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Nova senha deve ter no minimo 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmacao de nova senha e obrigatoria")]
    [Compare("NewPassword", ErrorMessage = "Senhas nao conferem")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email e obrigatorio")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha e obrigatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Nova senha deve ter no minimo 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmacao de nova senha e obrigatoria")]
    [Compare("NewPassword", ErrorMessage = "Senhas nao conferem")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// Response DTOs
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    /// <summary>Detalhe tecnico do erro (ex.: excecao do banco). Exibido para ajudar a corrigir.</summary>
    public string? ErrorDetail { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfoDto? User { get; set; }
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsSiteAdmin { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanInviteMembers { get; set; }
    public bool CanManageRoles { get; set; }
    public bool CanRemoveMembers { get; set; }
}

public class UpdateProfileDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
    public string? Name { get; set; }

    [Url(ErrorMessage = "URL do avatar invalida")]
    public string? AvatarUrl { get; set; }
}

// Invite DTOs
public class InviteUserDto
{
    [Required(ErrorMessage = "Email e obrigatorio")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";

    /// <summary>When set, invite is for this custom role; otherwise Role (Admin/Member) is used.</summary>
    public Guid? CustomRoleId { get; set; }

    /// <summary>Optional password for the invite; if set, the invitee must provide it when registering with the invite code.</summary>
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Senha do convite deve ter entre 4 e 100 caracteres")]
    public string? InvitePassword { get; set; }
}

public class InviteResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool HasPassword { get; set; }
}

public class AcceptInviteDto
{
    [Required(ErrorMessage = "Codigo do convite e obrigatorio")]
    public string InviteCode { get; set; } = string.Empty;
    public string? InvitePassword { get; set; }
}

public class MyOrganizationItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class OrganizationMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}

// Organization custom role DTOs
public class OrganizationRoleDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class CreateOrganizationRoleDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class UpdateOrganizationRoleDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
