namespace SmartMeetingManager.Domain.Entities;

public enum IntegrationType
{
    GoogleCalendar,
    Outlook,
    Gmail,
    MicrosoftGraph,
    Jira,
    Trello,
    AzureDevOps,
    Slack,
    Teams
}

public enum IntegrationStatus
{
    Active,
    Inactive,
    Error
}

public class Integration
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public IntegrationType Type { get; set; }
    public IntegrationStatus Status { get; set; }
    public string? AccessToken { get; set; } // Encrypted
    public string? RefreshToken { get; set; } // Encrypted
    public DateTime? TokenExpiresAt { get; set; }
    public Dictionary<string, string>? Settings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}