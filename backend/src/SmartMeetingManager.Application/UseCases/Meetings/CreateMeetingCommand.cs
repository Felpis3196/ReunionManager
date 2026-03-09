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
        // Parse and validate ScheduledAt (ISO 8601 format)
        if (!DateTime.TryParse(dto.ScheduledAt, out var scheduledAtParsed))
        {
            throw new ArgumentException("Data e hora inválidas. Use formato ISO 8601 (ex: 2024-01-20T14:30:00)");
        }

        // Convert to UTC for PostgreSQL (PostgreSQL only accepts UTC timestamps)
        DateTime scheduledAt;
        if (scheduledAtParsed.Kind == DateTimeKind.Unspecified)
        {
            // Assume local time and convert to UTC
            scheduledAt = DateTime.SpecifyKind(scheduledAtParsed, DateTimeKind.Local).ToUniversalTime();
        }
        else if (scheduledAtParsed.Kind == DateTimeKind.Local)
        {
            scheduledAt = scheduledAtParsed.ToUniversalTime();
        }
        else
        {
            // Already UTC
            scheduledAt = scheduledAtParsed;
        }

        // Parse and validate Duration (format: HH:mm or just TimeSpan string)
        if (!TimeSpan.TryParse(dto.Duration, out var duration))
        {
            throw new ArgumentException("Duração inválida. Use formato HH:mm (ex: 01:30)");
        }

        if (duration.TotalMinutes < 1)
        {
            throw new ArgumentException("Duração deve ser de pelo menos 1 minuto");
        }

        // Validate ScheduledAt is in the future (compare in UTC)
        if (scheduledAt < DateTime.UtcNow.AddMinutes(-5)) // 5 minutes tolerance for clock skew
        {
            throw new ArgumentException("Data e hora devem ser no futuro");
        }

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
            ScheduledAt = scheduledAt,
            Duration = duration,
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