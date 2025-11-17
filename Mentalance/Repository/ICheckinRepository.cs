
using Mentalance.Models;

namespace Mentalance.Repository
{
    /// <summary>
    /// Interface que define os contratos de acesso a dados para a entidade Checkin
    /// </summary>
    public interface ICheckinRepository
    {
        /// <summary>
        /// Busca todos os checkins realizados
        /// </summary>
        Task<IEnumerable<Checkin>> GetAllAsync();

        /// <summary>
        /// Busca check-ins de um usuário em um período específico
        /// </summary>
        /// <param name="idUsuario">ID do usuário</param>
        /// <param name="dataInicio">Data de início</param>
        /// <param name="dataFim">Data de fim</param>
        Task<IEnumerable<Checkin>> GetByUsuarioEPeriodoAsync(int idUsuario);

        /// <summary>
        /// Busca um checkin pelo ID
        /// </summary>
        /// <param name="id">ID do Checkin</param>
        Task<Checkin?> GetByIdAsync(int id);

        /// <summary>
        /// Adiciona um novo checkin ao banco de dados
        /// </summary>
        /// <param name="checkin">Objeto Checkin a ser adicionado</param>
        Task<Checkin> AddAsync(Checkin checkin);

        /// <summary>
        /// Atualiza um Checkin existente
        /// </summary>
        /// <param name="checkin">Objeto Checkin com dados atualizados</param>
        Task UpdateAsync(Checkin checkin);

        /// <summary>
        /// Remove um Checkin do banco de dados pelo ID
        /// </summary>
        /// <param name="id">ID do Checkin a ser removido</param>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Verifica se um checkin existe pelo ID
        /// </summary>
        /// <param name="id">ID do Checkin</param>
        Task<bool> CheckinExisteAsync(int id);

    }
}