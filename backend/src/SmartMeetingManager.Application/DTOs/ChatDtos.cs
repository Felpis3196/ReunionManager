using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Application.DTOs;

public class SendMessageDto
{
    [Required(ErrorMessage = "Texto da mensagem e obrigatorio")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Mensagem deve ter entre 1 e 4000 caracteres")]
    public string Text { get; set; } = string.Empty;
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
