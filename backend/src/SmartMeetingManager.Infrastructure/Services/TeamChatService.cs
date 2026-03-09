using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Services;

public class TeamChatService : ITeamChatService
{
    private readonly ApplicationDbContext _context;

    public TeamChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsMemberAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.OrganizationMembers
            .AnyAsync(m => m.UserId == userId && m.OrganizationId == organizationId && m.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(Guid organizationId, Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var isMember = await IsMemberAsync(userId, organizationId, cancellationToken);
        if (!isMember)
            return Array.Empty<ChatMessageDto>();

        var messages = await _context.TeamMessages
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Include(m => m.User)
            .ToListAsync(cancellationToken);

        return messages.OrderBy(m => m.CreatedAt).Select(MapToDto).ToList();
    }

    public async Task<ChatMessageDto?> SendMessageAsync(Guid organizationId, Guid userId, string text, CancellationToken cancellationToken = default)
    {
        var isMember = await IsMemberAsync(userId, organizationId, cancellationToken);
        if (!isMember)
            return null;

        var msg = new TeamMessage
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = userId,
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.TeamMessages.Add(msg);
        await _context.SaveChangesAsync(cancellationToken);

        var withUser = await _context.TeamMessages
            .AsNoTracking()
            .Include(m => m.User)
            .FirstAsync(m => m.Id == msg.Id, cancellationToken);
        return MapToDto(withUser);
    }

    private static ChatMessageDto MapToDto(TeamMessage m)
    {
        return new ChatMessageDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = m.User?.Name ?? "",
            UserAvatarUrl = m.User?.AvatarUrl,
            Text = m.Text,
            CreatedAt = m.CreatedAt
        };
    }
}
