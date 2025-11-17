using Mentalance.Models;

namespace Mentalance.Repository
{
    /// <summary>
    /// Interface que define os contratos de acesso a dados para a entidade AnaliseSemanal
    /// </summary>
    public interface IAnaliseSemanalRepository
    {

        /// <summary>
        /// Busca todas as análises realizadas
        /// </summary>
        Task<IEnumerable<AnaliseSemanal>> GetAllAsync();


        /// <summary>
        /// Busca uma analise pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        Task<AnaliseSemanal?> GetByIdAsync(int id);


        /// <summary>
        /// Adiciona uma análise semanal ao banco de dados
        /// </summary>
        /// <param name="analiseSemanal">Objeto AnaliseSemanal a ser adicionado</param>
        Task<AnaliseSemanal> AddAsync(AnaliseSemanal analiseSemanal);

        /// <summary>
        /// Remove uma análise do banco de dados pelo ID
        /// </summary>
        /// <param name="id">ID do usuário a ser removido</param>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Verifica se uma análise existe pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        Task<bool> ExisteAsync(int id);

    }
}