using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
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
}

public class UpdateUserDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
    public string? Name { get; set; }

    [Url(ErrorMessage = "URL do avatar invalida")]
    public string? AvatarUrl { get; set; }
}
