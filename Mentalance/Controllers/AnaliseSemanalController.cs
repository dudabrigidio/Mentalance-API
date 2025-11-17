using Mentalance.Models;
using Mentalance.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Mentalance.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar as análises semanais geradas automaticamente
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AnaliseSemanalController : ControllerBase
    {
        private readonly IAnaliseSemanalService _service;
        private readonly ILogger<AnaliseSemanalController> _logger;

        /// <summary>
        /// Inicializa uma nova instância do AnaliseSemanalController
        /// </summary>
        /// <param name="service">Serviço de análises semanais</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public AnaliseSemanalController(IAnaliseSemanalService service, ILogger<AnaliseSemanalController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lista de todas as análises realizadas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnaliseSemanal>>> GetCheckin()
        {
            _logger.LogInformation("Iniciando busca de todas as análises semanais");

            try
            {
                var analises = await _service.GetAllAsync();
                _logger.LogInformation("Análises semanais encontradas: {AnalisesCount}", analises.Count());
                return Ok(analises);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as análises semanais");
                throw;
            }
        }

        /// <summary>
        /// Busca de análise pelo ID
        /// </summary>
        /// <param name="id">ID de AnaliseSemanal</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<AnaliseSemanal>> GetAnaliseSemanal(int id)
        {
            _logger.LogInformation("Iniciando busca de análise semanal com ID: {AnaliseId}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar análise semanal com ID inválido: {AnaliseId}", id);
                return NotFound();
            }

            try
            {
                var analiseSemanal = await _service.GetByIdAsync(id);

                if (analiseSemanal == null)
                {
                    _logger.LogWarning("Análise semanal não encontrada com ID: {AnaliseId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Análise semanal encontrada: {AnaliseId}", analiseSemanal.IdAnalise);
                return Ok(analiseSemanal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar análise semanal com ID: {AnaliseId}", id);
                throw;
            }
        }



        /// <summary>
        /// Gera uma análise semanal para um usuário baseada nos check-ins dos últimos 7 dias
        /// </summary>
        /// <param name="idUsuario">ID do usuário para o qual a análise será gerada</param>
        [HttpPost]
        public async Task<ActionResult<AnaliseSemanal>> PostAnaliseSemanal(int idUsuario)
        {
            _logger.LogInformation("Iniciando geração de análise semanal para usuário: {UsuarioId}", idUsuario);

            try
            {
                var novaAnalise = await _service.GerarAnaliseSemanalAsync(idUsuario);
                _logger.LogInformation("Análise semanal gerada com sucesso: {AnaliseId} para usuário: {UsuarioId}", novaAnalise.IdAnalise, idUsuario);
                return CreatedAtAction("GetAnaliseSemanal", new { id = novaAnalise.IdAnalise }, novaAnalise);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erro ao gerar análise semanal para usuário: {UsuarioId}", idUsuario);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao gerar análise semanal para usuário: {UsuarioId}", idUsuario);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar análise semanal para usuário: {UsuarioId}", idUsuario);
                throw;
            }
        }



        /// <summary>
        /// Apagar Analise pelo ID
        /// </summary>
        /// <param name="id">ID da AnaliseSemanal</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnaliseSemanal(int id)
        {
            _logger.LogInformation("Iniciando exclusão de análise semanal com ID: {AnaliseId}", id);

            try
            {
                await _service.DeleteAsync(id);
                _logger.LogInformation("Análise semanal excluída com sucesso: {AnaliseId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Erro ao excluir análise semanal com ID: {AnaliseId} - Análise não encontrada", id);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao excluir análise semanal com ID: {AnaliseId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir análise semanal com ID: {AnaliseId}", id);
                throw;
            }
        }
    }
}