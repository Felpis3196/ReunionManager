using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Mappings;
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
        var participants = new List<Domain.Entities.MeetingParticipant>();
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
            participants.Add(participant);
            meeting.Participants.Add(participant);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load meeting with details for mapping
        var createdMeeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meeting.Id, cancellationToken);
        
        if (createdMeeting == null)
            throw new InvalidOperationException("Failed to retrieve created meeting");

        // Map to DTO using mapper
        return Mappings.MeetingMapper.ToDto(createdMeeting);
    }
}