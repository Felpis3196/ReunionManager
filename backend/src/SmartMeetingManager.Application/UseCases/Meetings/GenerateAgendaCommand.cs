using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Application.UseCases.Meetings;

public class GenerateAgendaCommand
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiService _aiService;

    public GenerateAgendaCommand(IUnitOfWork unitOfWork, IAiService aiService)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
    }

    public async Task<string> ExecuteAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(meetingId, cancellationToken);
        
        if (meeting == null)
            throw new ArgumentException("Meeting not found", nameof(meetingId));

        // Gather context for AI (emails, tasks, previous decisions, etc.)
        var contextItems = new List<string>();
        
        // TODO: Integrate with email service to get recent emails
        // TODO: Get pending tasks from project
        // TODO: Get recent decisions from related meetings

        // Generate agenda using AI
        var suggestedAgenda = await _aiService.GenerateAgendaAsync(
            meeting.Title,
            meeting.Description,
            contextItems,
            cancellationToken
        );

        meeting.SuggestedAgenda = suggestedAgenda;
        await _unitOfWork.Meetings.UpdateAsync(meeting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return suggestedAgenda;
    }
}