using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Application.UseCases.Meetings;

public class UpdateMeetingCommand
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMeetingCommand(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MeetingDto?> ExecuteAsync(Guid meetingId, UpdateMeetingDto dto, CancellationToken cancellationToken = default)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(meetingId, cancellationToken);
        
        if (meeting == null)
            return null;

        // Update properties if provided
        if (!string.IsNullOrWhiteSpace(dto.Title))
            meeting.Title = dto.Title;
        
        if (dto.Description != null)
            meeting.Description = dto.Description;
        
        if (dto.Type.HasValue)
            meeting.Type = dto.Type.Value;
        
        if (dto.ScheduledAt.HasValue)
            meeting.ScheduledAt = dto.ScheduledAt.Value;
        
        if (dto.Duration.HasValue)
            meeting.Duration = dto.Duration.Value;
        
        if (dto.Location != null)
            meeting.Location = dto.Location;
        
        if (dto.MeetingUrl != null)
            meeting.MeetingUrl = dto.MeetingUrl;
        
        if (dto.FinalAgenda != null)
            meeting.FinalAgenda = dto.FinalAgenda;

        meeting.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Meetings.UpdateAsync(meeting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return updated meeting
        var getMeetingQuery = new GetMeetingByIdQuery(_unitOfWork);
        return await getMeetingQuery.ExecuteAsync(meetingId, cancellationToken);
    }
}
