using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Mappings;
using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Application.UseCases.Meetings;

public class GetAllMeetingsQuery
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllMeetingsQuery(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MeetingDto>> ExecuteAsync(Guid? organizationId = null, Guid? projectId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Domain.Entities.Meeting> meetings;

        if (projectId.HasValue)
        {
            meetings = await _unitOfWork.Meetings.GetByProjectIdAsync(projectId.Value, cancellationToken);
        }
        else if (organizationId.HasValue)
        {
            meetings = await _unitOfWork.Meetings.GetByOrganizationIdAsync(organizationId.Value, cancellationToken);
        }
        else
        {
            meetings = await _unitOfWork.Meetings.GetAllAsync(cancellationToken);
        }

        // Map to DTOs (simplified - in production, use AutoMapper)
        var result = new List<MeetingDto>();
        
        foreach (var meeting in meetings)
        {
            var meetingWithDetails = await _unitOfWork.Meetings.GetWithDetailsAsync(meeting.Id, cancellationToken);
            if (meetingWithDetails != null)
            {
                result.Add(MeetingMapper.ToDto(meetingWithDetails));
            }
        }

        return result;
    }
}