using Mentalance.Models;
using Mentalance.Service;
using Microsoft.AspNetCore.Mvc;

namespace Mentalance.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar os Checkins emocionais
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CheckinsController : ControllerBase
    {
        private readonly ICheckinService _service;
        private readonly ILogger<CheckinsController> _logger;

        /// <summary>
        /// Inicializa uma nova instância do CheckinsController
        /// </summary>
        /// <param name="service">Serviço de check-ins</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public CheckinsController(ICheckinService service, ILogger<CheckinsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os CheckIn's de um usuário específico
        /// </summary>
        /// <param name="IdUsuario">ID do usuário</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Checkin>>> GetAllCheckin([FromQuery] int idUsuario)
        {
            _logger.LogInformation("Iniciando busca de checkins do usuário: {idUsuario}", idUsuario);

            if (idUsuario <= 0)
            {
                _logger.LogWarning("Tentativa de buscar checkins com ID do usuário inválido: {idUsuario}", idUsuario);
                return BadRequest(new { message = "ID do usuário deve ser maior que zero" });
            }

            try {
                var checkins = await _service.GetAllAsync(idUsuario);
                _logger.LogInformation("Checkins encontrados para o usuário {idUsuario}: {CheckinsCount}", idUsuario, checkins.Count());
                return Ok(checkins);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao buscar checkins do usuário: {idUsuario}", idUsuario);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar checkins do usuário: {idUsuario}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Busca de CheckIn pelo ID
        /// </summary>
        /// <param name="id">ID do CheckIn</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Checkin>> GetCheckin(int id)
        {
            _logger.LogInformation("Iniciando busca de checkin com ID: {CheckinId}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar checkin com ID inválido: {CheckinId}", id);
                return NotFound();
            }
            try {
                var checkin = await _service.GetByIdAsync(id);
                _logger.LogInformation("Checkin encontrado: {CheckinId}", checkin?.IdCheckin);
                return Ok(checkin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar checkin com ID: {CheckinId}", id);
                throw;
            }
        }
                



        /// <summary>
        /// Realização de CheckIn
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST/api/CheckIn
        ///        {
        ///             "idUsuario": 1,
        ///             "emocao": "Feliz",
        ///             "texto": "Hoje estou feliz porque consegui resolver o problema do cliente"
        ///         }
        /// 
        ///       Valores para emoção: Feliz, Cansado, Ansioso, Calmo, Estressado
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<Checkin>> PostCheckin(CheckinDto checkinDto)
        {
            _logger.LogInformation("Iniciando criação de checkin");
            try {
            
                var novoCheckin = await _service.CreateAsync(checkinDto);
                _logger.LogInformation("Checkin criado com sucesso: {CheckinId}", novoCheckin.IdCheckin);
                return CreatedAtAction("GetCheckin", new { id = novoCheckin.IdCheckin }, novoCheckin);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erro ao criar checkin");
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao criar checkin");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar checkin");
                throw;
            }
        }

        /// <summary>
        /// Alteração de CheckIn já realizado
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     PUT/api/CheckIn
        ///        {
        ///             "idUsuario": 1,
        ///             "emocao": "Feliz",
        ///             "texto": "Hoje estou feliz porque consegui resolver o problema do cliente"
        ///         }
        /// 
        ///       Valores para emoção: Feliz, Cansado, Ansioso, Calmo, Estressado
        ///       
        /// </remarks>
        /// <param name="id">ID do CheckIn</param>
        /// <param name="checkinDto">Dados do check-in para atualização</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCheckin(int id, CheckinDto checkinDto)
        {
            
            _logger.LogInformation("Iniciando atualização de checkin com ID: {CheckinId}", id);
            try {
            
                await _service.UpdateAsync(id, checkinDto);
                _logger.LogInformation("Checkin atualizado com sucesso: {CheckinId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar checkin com ID: {CheckinId} - Checkin não encontrado", id);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar checkin com ID: {CheckinId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar checkin com ID: {CheckinId}", id);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar checkin com ID: {CheckinId}", id);
                throw;
            }
        }


        /// <summary>
        /// Apagar CheckIn pelo ID
        /// </summary>
        /// <param name="id">ID do CheckIn</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCheckin(int id)
        {
            _logger.LogInformation("Iniciando exclusão de checkin com ID: {CheckinId}", id);
            try {
            
                await _service.DeleteAsync(id);
                _logger.LogInformation("Checkin excluído com sucesso: {CheckinId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Erro ao excluir checkin com ID: {CheckinId} - Checkin não encontrado", id);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao excluir checkin com ID: {CheckinId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir checkin com ID: {CheckinId}", id);
                throw;
            }
        }
    }
}