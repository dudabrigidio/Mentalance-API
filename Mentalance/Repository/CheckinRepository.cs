using Mentalance.Connection;
using Mentalance.Models;
using Mentalance.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mentalance.Repository
{
    /// <summary>
    /// Implementação do repositório para a entidade Checkin
    /// Responsável apenas pelo acesso direto aos dados no banco
    /// </summary>
    public class CheckinRepository : ICheckinRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CheckinRepository> _logger;

        /// <summary>
        /// Inicializa uma nova instância do CheckinRepository
        /// </summary>
        /// <param name="context">Contexto do banco de dados</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public CheckinRepository(AppDbContext context, ILogger<CheckinRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca todos os checkins realizados
        /// </summary>
        public async Task<IEnumerable<Checkin>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os checkins");
            
            try {
                return await _context.Checkin.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os checkins");
                throw;
            }
        }

        /// <summary>
        /// Busca todos os checkins de um usuário específico
        /// </summary>
        public async Task<IEnumerable<Checkin>> GetByUsuarioAsync(int idUsuario)
        {
            _logger.LogInformation("Buscando checkins do usuário: {UsuarioId}", idUsuario);
            
            try {
                var checkins = await _context.Checkin
                    .Where(c => c.IdUsuario == idUsuario)
                    .OrderByDescending(c => c.DataCheckin)
                    .ToListAsync();
                
                _logger.LogInformation("Checkins encontrados para o usuário {UsuarioId}: {CheckinsCount}", idUsuario, checkins.Count());
                return checkins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar checkins do usuário: {UsuarioId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Busca checkins de um usuário em um período específico
        /// </summary>
        public async Task<IEnumerable<Checkin>> GetByUsuarioEPeriodoAsync(int idUsuario)
        {
            _logger.LogInformation("Buscando checkins de um usuário em um período específico");
            
            try {
                var dataFim = DateTime.Now;
                var dataInicio = dataFim.AddDays(-7);
                // AsNoTracking() melhora performance para queries de leitura (não precisa rastrear mudanças)
                var checkins = await _context.Checkin
                    .AsNoTracking()
                    .Where(c => c.IdUsuario == idUsuario 
                        && c.DataCheckin >= dataInicio 
                        && c.DataCheckin <= dataFim)
                    .OrderBy(c => c.DataCheckin)
                    .ToListAsync();
                _logger.LogInformation("Checkins encontrados: {CheckinsCount}", checkins.Count());
                return checkins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar checkins de um usuário em um período específico");
                throw;
            }
        }

        /// <summary>
        /// Busca um checkin pelo ID
        /// </summary>
        public async Task<Checkin?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Buscando checkin pelo ID: {CheckinId}", id);
            
            try {
                var checkin = await _context.Checkin.FindAsync(id);
                
                if (checkin == null)
                {
                    _logger.LogDebug("Checkin não encontrado: {CheckinId}", id);
                }
                else
                {
                    _logger.LogDebug("Checkin encontrado: {CheckinId}", checkin.IdCheckin);
                }
                
                return checkin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar checkin pelo ID: {CheckinId}", id);
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo checkin ao banco de dados
        /// </summary>
        public async Task<Checkin> AddAsync(Checkin checkin)
        {
            _logger.LogDebug("Persistindo novo checkin no banco de dados. Usuário: {UsuarioId}", checkin.IdUsuario);
            
            try {
                _context.Checkin.Add(checkin);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Checkin persistido com sucesso: {CheckinId}", checkin.IdCheckin);
                return checkin;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao persistir checkin no banco de dados. Usuário: {UsuarioId}", checkin.IdUsuario);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar novo checkin ao banco de dados");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um checkin existente
        /// </summary>
        public async Task UpdateAsync(Checkin checkin)
        {
            _logger.LogDebug("Atualizando checkin no banco de dados. ID: {CheckinId}", checkin.IdCheckin);
            
            try {
                _context.Entry(checkin).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogDebug("Checkin atualizado com sucesso: {CheckinId}", checkin.IdCheckin);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar checkin no banco de dados. ID: {CheckinId}", checkin.IdCheckin);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar checkin no banco de dados. ID: {CheckinId}", checkin.IdCheckin);
                throw;
            }
        }

        /// <summary>
        /// Remove um checkin do banco de dados pelo ID
        /// </summary>
        /// <returns>True se o checkin foi removido, False se não foi encontrado</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogDebug("Iniciando exclusão de checkin. ID: {CheckinId}", id);
            
            try
            {
                // Busca o checkin pelo ID
                var checkin = await GetByIdAsync(id);

                // Se não encontrou, retorna false
                if (checkin == null)
                {
                    _logger.LogWarning("Checkin não encontrado para exclusão: {CheckinId}", id);
                    return false;
                }

                // Remove o checkin encontrado
                _context.Checkin.Remove(checkin);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Checkin removido com sucesso: {CheckinId}", id);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao remover checkin no banco de dados. ID: {CheckinId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover checkin. ID: {CheckinId}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se um checkin existe pelo ID
        /// </summary>
        public async Task<bool> CheckinExisteAsync(int id)
        {
            _logger.LogDebug("Verificando se checkin existe pelo ID: {CheckinId}", id);
            
            try {
                var checkin = await _context.Checkin
                    .Where(e => e.IdCheckin == id)
                    .Select(e => e.IdCheckin)
                    .FirstOrDefaultAsync();
                
                var existe = checkin > 0;
                _logger.LogDebug("Checkin {CheckinId} existe: {Existe}", id, existe);
                return existe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se checkin existe pelo ID: {CheckinId}", id);
                throw;
            }
        }


    }
}