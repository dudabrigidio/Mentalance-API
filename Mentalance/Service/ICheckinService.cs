using Mentalance.Models;

namespace Mentalance.Service
{
    /// <summary>
    /// Interface que define os contratos de lógica de negócio para a entidade Checkin
    /// </summary>
    public interface ICheckinService
    {
        /// <summary>
        /// Retorna todos os checkins realizados
        /// </summary>
        Task<IEnumerable<Checkin>> GetAllAsync();

        /// <summary>
        /// Busca um checkin pelo ID
        /// </summary>
        /// <param name="id">ID do checkin</param>
        Task<Checkin?> GetByIdAsync(int id);

        /// <summary>
        /// Cria um novo checkin após validar os dados
        /// </summary>
        /// <param name="checkin">Objeto Checkin a ser criado</param>
        Task<Checkin> CreateAsync(CheckinDto checkinDto);

        /// <summary>
        /// Atualiza um checkin existente
        /// </summary>
        /// <param name="id">ID do checkin a ser atualizado</param>
        /// <param name="checkin">Objeto checkin com dados atualizados</param>
        Task UpdateAsync(int id, CheckinDto checkinDto);

        /// <summary>
        /// Remove um checkin do sistema
        /// </summary>
        /// <param name="id">ID do checkin a ser removido</param>
        Task DeleteAsync(int id);
    }
}