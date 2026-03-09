using SmartMeetingManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Application.DTOs;

public record CreateMeetingDto
{
    public Guid OrganizationId { get; init; }
    public Guid? ProjectId { get; init; }
    
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string Title { get; init; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string? Description { get; init; }
    
    [Required(ErrorMessage = "Tipo é obrigatório")]
    public MeetingType Type { get; init; }
    
    [Required(ErrorMessage = "Data e hora são obrigatórias")]
    public string ScheduledAt { get; init; } = string.Empty;
    
    [Required(ErrorMessage = "Duração é obrigatória")]
    public string Duration { get; init; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Localização deve ter no máximo 500 caracteres")]
    public string? Location { get; init; }
    
    [Url(ErrorMessage = "URL deve ser válida")]
    [StringLength(1000, ErrorMessage = "URL deve ter no máximo 1000 caracteres")]
    public string? MeetingUrl { get; init; }
    
    public IEnumerable<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
}