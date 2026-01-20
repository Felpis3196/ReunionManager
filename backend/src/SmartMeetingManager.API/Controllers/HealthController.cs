using Microsoft.AspNetCore.Mvc;

namespace SmartMeetingManager.API.Controllers;

/// <summary>
/// Controller para verificação de saúde da API
/// </summary>
[ApiController]
[Route("api/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica o status de saúde da API
    /// </summary>
    /// <returns>Status da API</returns>
    /// <response code="200">API está funcionando</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
