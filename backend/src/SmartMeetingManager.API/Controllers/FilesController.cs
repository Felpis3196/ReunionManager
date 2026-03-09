using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para gerenciamento de arquivos em reunioes
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId}/files")]
[Produces("application/json")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FilesController> _logger;
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".jpg", ".jpeg", ".png", ".gif", ".mp3", ".mp4", ".zip" };

    public FilesController(
        IUnitOfWork unitOfWork,
        ILogger<FilesController> logger,
        IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Lista todos os arquivos de uma reuniao
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<FileDto>>> GetAll(
        [Required] Guid meetingId,
        CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(meetingId, cancellationToken);
        if (meeting == null)
            return NotFound(new { error = "Reuniao nao encontrada" });

        var files = await _unitOfWork.Files.GetByMeetingIdAsync(meetingId, cancellationToken);
        return Ok(files.Select(MapToDto));
    }

    /// <summary>
    /// Faz upload de um arquivo para a reuniao
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<FileDto>> Upload(
        [Required] Guid meetingId,
        IFormFile file,
        [FromForm] string? description,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Nenhum arquivo enviado" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { error = $"Arquivo muito grande. Maximo: {MaxFileSize / (1024 * 1024)}MB" });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new { error = $"Tipo de arquivo nao permitido. Extensoes permitidas: {string.Join(", ", AllowedExtensions)}" });

            var meeting = await _unitOfWork.Meetings.GetByIdAsync(meetingId, cancellationToken);
            if (meeting == null)
                return NotFound(new { error = "Reuniao nao encontrada" });

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            // Create storage directory
            var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads", meetingId.ToString());
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Create database record
            var meetingFile = new MeetingFile
            {
                Id = Guid.NewGuid(),
                MeetingId = meetingId,
                UploadedById = userId.Value,
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                StoragePath = filePath,
                Category = GetFileCategory(extension),
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Files.AddAsync(meetingFile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("File uploaded: {FileName} for meeting {MeetingId}", file.FileName, meetingId);

            return CreatedAtAction(nameof(GetAll), new { meetingId }, MapToDto(meetingFile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for meeting {MeetingId}", meetingId);
            return BadRequest(new { error = "Erro ao fazer upload do arquivo" });
        }
    }

    /// <summary>
    /// Faz download de um arquivo
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        [Required] Guid meetingId,
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        var file = await _unitOfWork.Files.GetByIdAsync(id, cancellationToken);
        if (file == null || file.MeetingId != meetingId)
            return NotFound(new { error = "Arquivo nao encontrado" });

        if (!System.IO.File.Exists(file.StoragePath))
            return NotFound(new { error = "Arquivo nao encontrado no servidor" });

        var stream = new FileStream(file.StoragePath, FileMode.Open, FileAccess.Read);
        return File(stream, file.ContentType, file.OriginalFileName);
    }

    /// <summary>
    /// Exclui um arquivo
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [Required] Guid meetingId,
        [Required] Guid id,
        CancellationToken cancellationToken)
    {
        var file = await _unitOfWork.Files.GetByIdAsync(id, cancellationToken);
        if (file == null || file.MeetingId != meetingId)
            return NotFound(new { error = "Arquivo nao encontrado" });

        // Delete physical file
        if (System.IO.File.Exists(file.StoragePath))
        {
            try
            {
                System.IO.File.Delete(file.StoragePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete physical file: {Path}", file.StoragePath);
            }
        }

        // Delete database record
        await _unitOfWork.Files.DeleteAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    private static FileCategory GetFileCategory(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" or ".doc" or ".docx" or ".txt" => FileCategory.Document,
            ".ppt" or ".pptx" => FileCategory.Presentation,
            ".xls" or ".xlsx" or ".csv" => FileCategory.Spreadsheet,
            ".jpg" or ".jpeg" or ".png" or ".gif" => FileCategory.Image,
            ".mp3" or ".wav" => FileCategory.Audio,
            ".mp4" or ".avi" or ".mov" => FileCategory.Video,
            _ => FileCategory.Other
        };
    }

    private static FileDto MapToDto(MeetingFile file)
    {
        return new FileDto
        {
            Id = file.Id,
            MeetingId = file.MeetingId,
            UploadedById = file.UploadedById,
            FileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            Category = file.Category.ToString(),
            Description = file.Description,
            CreatedAt = file.CreatedAt
        };
    }
}

public class FileDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid UploadedById { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
