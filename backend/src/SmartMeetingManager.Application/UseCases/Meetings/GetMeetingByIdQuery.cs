using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Application.UseCases.Meetings;

public class GetMeetingByIdQuery
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeetingByIdQuery(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MeetingDto?> ExecuteAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
        
        if (meeting == null)
            return null;

        // Map to DTO using mapper
        return Mappings.MeetingMapper.ToDto(meeting);
    }
}