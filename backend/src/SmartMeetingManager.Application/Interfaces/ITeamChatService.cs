using SmartMeetingManager.Application.DTOs;

namespace SmartMeetingManager.Application.Interfaces;

public interface ITeamChatService
{
    Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(Guid organizationId, Guid userId, int limit = 50, CancellationToken cancellationToken = default);
    Task<ChatMessageDto?> SendMessageAsync(Guid organizationId, Guid userId, string text, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
}
