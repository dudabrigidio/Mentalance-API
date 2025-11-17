using Mentalance.Models;
using Mentalance.Dto;


namespace Mentalance.Service
{
    /// <summary>
    /// Interface que define os contratos de lógica de negócio para a entidade Usuario
    /// </summary>
    public interface IUsuarioService
    {
        /// <summary>
        /// Retorna todos os usuários cadastrados
        /// </summary>
        Task<IEnumerable<Usuario>> GetAllAsync();

        /// <summary>
        /// Busca um usuário pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        Task<Usuario?> GetByIdAsync(int id);

        /// <summary>
        /// Cria um novo usuário após validar os dados
        /// </summary>
        /// <param name="usuario">Objeto Usuario a ser criado</param>
        Task<Usuario> CreateAsync(UsuarioDto usuarioDto);

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        /// <param name="id">ID do usuário a ser atualizado</param>
        /// <param name="usuario">Objeto Usuario com dados atualizados</param>
        Task UpdateAsync(int id, UsuarioDto usuarioDto);

        /// <summary>
        /// Remove um usuário do sistema
        /// </summary>
        /// <param name="id">ID do usuário a ser removido</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Realiza o login de um usuário
        /// </summary>
        /// <param name="loginDto">Dados de login (email e senha)</param>
        /// <returns>Usuário autenticado ou null se credenciais inválidas</returns>
        Task<Usuario?> LoginAsync(LoginDto loginDto);
    }
}