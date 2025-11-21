using Mentalance.Models;

namespace Mentalance.Service
{
    /// <summary>
    /// Interface que define os contratos de lógica de negócio para a entidade AnaliseSemanal
    /// </summary>
    public interface IAnaliseSemanalService
    {

        /// <summary>
        /// Retorna todas as análises realizadas
        /// </summary>
        Task<IEnumerable<AnaliseSemanal>> GetAllAsync();


        /// <summary>
        /// Gera uma análise semanal usando ML.NET baseada nos últimos 7 dias
        /// </summary>
        /// <param name="idUsuario">ID do usuário</param>
        Task<AnaliseSemanal> GerarAnaliseSemanalAsync(int idUsuario);

        /// <summary>
        /// Busca análise semanal por ID
        /// </summary>
        /// <param name="id">ID da análise</param>
        Task<AnaliseSemanal?> GetByIdAsync(int id);

        /// <summary>
        /// Remove ua analise do sistema
        /// </summary>
        /// <param name="id">ID da análise a ser removido</param>
        Task DeleteAsync(int id);
    }
}
