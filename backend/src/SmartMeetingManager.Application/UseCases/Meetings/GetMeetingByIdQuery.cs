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

        // Map to DTO (simplified - in real implementation, use AutoMapper)
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
            SuggestedAgenda = meeting.SuggestedAgenda,
            FinalAgenda = meeting.FinalAgenda,
            Summary = meeting.Summary,
            CreatedAt = meeting.CreatedAt,
            StartedAt = meeting.StartedAt,
            EndedAt = meeting.EndedAt,
            OrganizerName = meeting.Organizer.Name,
            Participants = meeting.Participants.Select(p => new ParticipantDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.Name,
                UserEmail = p.User.Email,
                Status = p.Status,
                InvitedAt = p.InvitedAt,
                AttendedAt = p.AttendedAt
            }),
            AgendaItems = meeting.AgendaItems.Select(a => new AgendaItemDto
            {
                Id = a.Id,
                Order = a.Order,
                Title = a.Title,
                Description = a.Description,
                EstimatedDuration = a.EstimatedDuration,
                IsCompleted = a.IsCompleted
            }),
            Decisions = meeting.Decisions.Select(d => new DecisionDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                MadeAt = d.MadeAt,
                MadeById = d.MadeById,
                MadeByName = d.MadeBy?.Name,
                IsImplemented = d.IsImplemented
            }),
            Tasks = meeting.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                MeetingId = t.MeetingId,
                ProjectId = t.ProjectId,
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo.Name,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt
            })
        };
    }
}