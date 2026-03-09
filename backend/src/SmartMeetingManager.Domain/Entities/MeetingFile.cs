namespace SmartMeetingManager.Domain.Entities;

public class MeetingFile
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid UploadedById { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public FileCategory Category { get; set; } = FileCategory.Document;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Meeting Meeting { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}

public enum FileCategory
{
    Document,
    Presentation,
    Spreadsheet,
    Image,
    Audio,
    Video,
    Other
}
