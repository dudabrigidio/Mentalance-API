using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mentalance.Controllers;

/// <summary>
/// Controller responsável por verificar o status de saúde da aplicação e suas dependências
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    /// <summary>
    /// Inicializa uma nova instância do HealthController
    /// </summary>
    /// <param name="healthCheckService">Serviço de verificação de saúde da aplicação</param>
    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Verifica o status de saúde da aplicação e suas dependências
    /// </summary>
    /// <returns>Status de saúde da aplicação com detalhes de todos os checks</returns>
    /// <response code="200">Aplicação está saudável</response>
    /// <response code="503">Aplicação está com problemas ou dependências indisponíveis</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        var healthCheck = await _healthCheckService.CheckHealthAsync();

        // Retorna status HTTP baseado no health check
        var statusCode = healthCheck.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, new
        {
            status = healthCheck.Status.ToString(),
            checks = healthCheck.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.ToString()
            })
        });
    }

}
