namespace SmartMeetingManager.Domain.Interfaces;

public interface IAiService
{
    Task<string> GenerateAgendaAsync(string meetingTitle, string? description, IEnumerable<string> contextItems, CancellationToken cancellationToken = default);
    Task<string> GenerateSummaryAsync(string transcript, IEnumerable<string> agendaItems, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExtractedActions>> ExtractActionsAsync(string transcript, IEnumerable<Guid> participantIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> ExtractDecisionsAsync(string transcript, CancellationToken cancellationToken = default);
}

public record ExtractedActions(
    string Title,
    string Description,
    Guid? AssignedToId,
    DateTime? DueDate,
    string Priority
);