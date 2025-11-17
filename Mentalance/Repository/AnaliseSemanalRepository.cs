using Mentalance.Connection;
using Mentalance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mentalance.Repository
{
    /// <summary>
    /// Implementação do repositório para a entidade AnaliseSemanal
    /// Responsável apenas pelo acesso direto aos dados no banco
    /// </summary>
    public class AnaliseSemanalRepository : IAnaliseSemanalRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnaliseSemanalRepository> _logger;

        /// <summary>
        /// Inicializa uma nova instância do AnaliseSemanalRepository
        /// </summary>
        /// <param name="context">Contexto do banco de dados</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public AnaliseSemanalRepository(AppDbContext context, ILogger<AnaliseSemanalRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca todas as análises pelo ID
        /// </summary>
        public async Task<IEnumerable<AnaliseSemanal>> GetAllAsync()
        {
            _logger.LogDebug("Buscando todas as análises semanais no banco de dados");
            
            try
            {
                var analises = await _context.AnaliseSemanal.ToListAsync();
                _logger.LogDebug("Análises semanais encontradas: {AnalisesCount}", analises.Count());
                return analises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as análises semanais no banco de dados");
                throw;
            }
        }

        /// <summary>
        /// Busca uma análise pelo ID
        /// </summary>
        public async Task<AnaliseSemanal?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Buscando análise semanal pelo ID: {AnaliseId}", id);
            
            try
            {
                var analise = await _context.AnaliseSemanal.FindAsync(id);
                
                if (analise == null)
                {
                    _logger.LogDebug("Análise semanal não encontrada: {AnaliseId}", id);
                }
                else
                {
                    _logger.LogDebug("Análise semanal encontrada: {AnaliseId}", analise.IdAnalise);
                }
                
                return analise;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar análise semanal pelo ID: {AnaliseId}", id);
                throw;
            }
        }


        /// <summary>
        /// Adiciona uma nova análise ao banco de dados
        /// </summary>
        public async Task<AnaliseSemanal> AddAsync(AnaliseSemanal analiseSemanal)
        {
            _logger.LogDebug("Persistindo nova análise semanal no banco de dados. Usuário: {UsuarioId}", analiseSemanal.IdUsuario);
            
            try
            {
                _context.AnaliseSemanal.Add(analiseSemanal);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Análise semanal persistida com sucesso: {AnaliseId}, Usuário: {UsuarioId}", analiseSemanal.IdAnalise, analiseSemanal.IdUsuario);
                return analiseSemanal;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao persistir análise semanal no banco de dados. Usuário: {UsuarioId}", analiseSemanal.IdUsuario);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar nova análise semanal no banco de dados. Usuário: {UsuarioId}", analiseSemanal.IdUsuario);
                throw;
            }
        }


        /// <summary>
        /// Remove uma análise do banco de dados pelo ID
        /// </summary>
        /// <returns>True se a análise foi removida, False se não foi encontrada</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogDebug("Iniciando exclusão de análise semanal. ID: {AnaliseId}", id);
            
            try
            {
                // Busca a análise pelo ID
                var analiseSemanal = await GetByIdAsync(id);

                // Se não encontrou, retorna false
                if (analiseSemanal == null)
                {
                    _logger.LogWarning("Análise semanal não encontrada para exclusão: {AnaliseId}", id);
                    return false;
                }

                // Remove a análise encontrada
                _context.AnaliseSemanal.Remove(analiseSemanal);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Análise semanal removida com sucesso: {AnaliseId}", id);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao remover análise semanal no banco de dados. ID: {AnaliseId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover análise semanal. ID: {AnaliseId}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se uma análise existe pelo ID
        /// </summary>
        public async Task<bool> ExisteAsync(int id)
        {
            _logger.LogDebug("Verificando se análise semanal existe pelo ID: {AnaliseId}", id);
            
            try
            {
                var analise = await _context.AnaliseSemanal
                    .Where(e => e.IdAnalise == id)
                    .Select(e => e.IdAnalise)
                    .FirstOrDefaultAsync();
                
                var existe = analise > 0;
                _logger.LogDebug("Análise semanal {AnaliseId} existe: {Existe}", id, existe);
                return existe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se análise semanal existe pelo ID: {AnaliseId}", id);
                throw;
            }
        }

        
    }
}