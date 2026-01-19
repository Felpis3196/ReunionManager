using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Application.UseCases.Meetings;

public class ProcessTranscriptCommand
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiService _aiService;

    public ProcessTranscriptCommand(IUnitOfWork unitOfWork, IAiService aiService)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
    }

    public async Task ExecuteAsync(Guid meetingId, string transcriptContent, CancellationToken cancellationToken = default)
    {
        var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
        
        if (meeting == null)
            throw new ArgumentException("Meeting not found", nameof(meetingId));

        // Save transcript
        var transcript = new Domain.Entities.Transcript
        {
            Id = Guid.NewGuid(),
            MeetingId = meetingId,
            Content = transcriptContent,
            ProcessedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Transcripts.AddAsync(transcript, cancellationToken);

        // Generate summary
        var agendaItems = meeting.AgendaItems.Select(a => $"{a.Order}. {a.Title}").ToList();
        var summary = await _aiService.GenerateSummaryAsync(transcriptContent, agendaItems, cancellationToken);
        meeting.Summary = summary;

        // Extract decisions
        var decisions = await _aiService.ExtractDecisionsAsync(transcriptContent, cancellationToken);
        foreach (var decisionText in decisions)
        {
            var decision = new Domain.Entities.Decision
            {
                Id = Guid.NewGuid(),
                MeetingId = meetingId,
                Title = decisionText,
                Description = decisionText,
                MadeAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            meeting.Decisions.Add(decision);
        }

        // Extract and create actions/tasks
        var participantIds = meeting.Participants.Select(p => p.UserId).ToList();
        var actions = await _aiService.ExtractActionsAsync(transcriptContent, participantIds, cancellationToken);
        
        foreach (var action in actions)
        {
            var assignedToId = action.AssignedToId ?? meeting.Participants.FirstOrDefault()?.UserId ?? meeting.OrganizerId;
            
            var task = new Domain.Entities.Task
            {
                Id = Guid.NewGuid(),
                MeetingId = meetingId,
                ProjectId = meeting.ProjectId,
                AssignedToId = assignedToId,
                Title = action.Title,
                Description = action.Description,
                Status = Domain.Entities.TaskStatus.Pending,
                Priority = Enum.Parse<Domain.Entities.TaskPriority>(action.Priority, ignoreCase: true),
                DueDate = action.DueDate,
                CreatedAt = DateTime.UtcNow
            };
            meeting.Tasks.Add(task);
        }

        meeting.Status = Domain.Entities.MeetingStatus.Completed;
        meeting.EndedAt = DateTime.UtcNow;

        await _unitOfWork.Meetings.UpdateAsync(meeting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}