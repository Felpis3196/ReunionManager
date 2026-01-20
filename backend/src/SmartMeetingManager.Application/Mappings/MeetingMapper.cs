using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Domain.Entities;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;

namespace SmartMeetingManager.Application.Mappings;

public static class MeetingMapper
{
    public static MeetingDto ToDto(Meeting meeting)
    {
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
            ProjectName = meeting.Project?.Name,
            OrganizerName = meeting.Organizer.Name,
            Participants = meeting.Participants.Select(ToParticipantDto).ToList(),
            AgendaItems = meeting.AgendaItems.Select(ToAgendaItemDto).ToList(),
            Decisions = meeting.Decisions.Select(ToDecisionDto).ToList(),
            Tasks = meeting.Tasks.Select(ToTaskDto).ToList()
        };
    }

    private static ParticipantDto ToParticipantDto(MeetingParticipant participant)
    {
        return new ParticipantDto
        {
            Id = participant.Id,
            UserId = participant.UserId,
            UserName = participant.User.Name,
            UserEmail = participant.User.Email,
            Status = participant.Status,
            InvitedAt = participant.InvitedAt,
            AttendedAt = participant.AttendedAt
        };
    }

    private static AgendaItemDto ToAgendaItemDto(AgendaItem agendaItem)
    {
        return new AgendaItemDto
        {
            Id = agendaItem.Id,
            Order = agendaItem.Order,
            Title = agendaItem.Title,
            Description = agendaItem.Description,
            EstimatedDuration = agendaItem.EstimatedDuration,
            IsCompleted = agendaItem.IsCompleted
        };
    }

    private static DecisionDto ToDecisionDto(Decision decision)
    {
        return new DecisionDto
        {
            Id = decision.Id,
            Title = decision.Title,
            Description = decision.Description,
            MadeAt = decision.MadeAt,
            MadeById = decision.MadeById,
            MadeByName = decision.MadeBy?.Name,
            IsImplemented = decision.IsImplemented
        };
    }

    private static TaskDto ToTaskDto(TaskEntity task)
    {
        return new TaskDto
        {
            Id = task.Id,
            MeetingId = task.MeetingId,
            ProjectId = task.ProjectId,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo.Name,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt
        };
    }
}
