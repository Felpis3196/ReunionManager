namespace SmartMeetingManager.Application.DTOs;

public record DecisionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime MadeAt { get; init; }
    public Guid? MadeById { get; init; }
    public string? MadeByName { get; init; }
    public bool IsImplemented { get; init; }
}