using Mentalance.Models;

namespace Mentalance.Repository
{
    /// <summary>
    /// Interface que define os contratos de acesso a dados para a entidade Usuario
    /// </summary>
    public interface IUsuarioRepository
    {
        /// <summary>
        /// Busca todos os usuários cadastrados
        /// </summary>
        Task<IEnumerable<Usuario>> GetAllAsync();

        /// <summary>
        /// Busca um usuário pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        Task<Usuario?> GetByIdAsync(int id);

        /// <summary>
        /// Busca um usuário pelo e-mail
        /// </summary>
        /// <param name="email">E-mail do usuário</param>
        Task<Usuario?> GetByEmailAsync(string email);

        /// <summary>
        /// Adiciona um novo usuário ao banco de dados
        /// </summary>
        /// <param name="usuario">Objeto Usuario a ser adicionado</param>
        Task<Usuario> AddAsync(Usuario usuario);

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        /// <param name="usuario">Objeto Usuario com dados atualizados</param>
        Task UpdateAsync(Usuario usuario);



        /// <summary>
        /// Remove um usuário do banco de dados pelo ID
        /// </summary>
        /// <param name="id">ID do usuário a ser removido</param>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Verifica se um usuário existe pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        Task<bool> ExisteAsync(int id);

        /// <summary>
        /// Verifica se já existe um usuário com o e-mail informado
        /// </summary>
        /// <param name="email">E-mail a ser verificado</param>
        Task<bool> EmailExisteAsync(string email);
    }
}