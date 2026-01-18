using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Application.UseCases.Meetings;

public class CreateMeetingCommand
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMeetingCommand(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MeetingDto> ExecuteAsync(CreateMeetingDto dto, Guid organizerId, CancellationToken cancellationToken = default)
    {
        var meeting = new Domain.Entities.Meeting
        {
            Id = Guid.NewGuid(),
            OrganizationId = dto.OrganizationId,
            ProjectId = dto.ProjectId,
            OrganizerId = organizerId,
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Status = Domain.Entities.MeetingStatus.Scheduled,
            ScheduledAt = dto.ScheduledAt,
            Duration = dto.Duration,
            Location = dto.Location,
            MeetingUrl = dto.MeetingUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Meetings.AddAsync(meeting, cancellationToken);

        // Add participants
        foreach (var participantId in dto.ParticipantIds)
        {
            var participant = new Domain.Entities.MeetingParticipant
            {
                Id = Guid.NewGuid(),
                MeetingId = meeting.Id,
                UserId = participantId,
                Status = Domain.Entities.ParticipantStatus.Invited,
                InvitedAt = DateTime.UtcNow
            };
            // Note: In real implementation, add to repository
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO (simplified)
        return new MeetingDto
        {
            Id = meeting.Id,
            OrganizationId = meeting.OrganizationId,
            ProjectId = meeting.ProjectId,
            OrganizerId = meeting.OrganizerId,
            Title = meeting.Title,
            Description = meeting.Description,
            Type = meeting.Type,
            Status = meeting.Status,
            ScheduledAt = meeting.ScheduledAt,
            Duration = meeting.Duration,
            Location = meeting.Location,
            MeetingUrl = meeting.MeetingUrl,
            CreatedAt = meeting.CreatedAt
        };
    }
}