using Mentalance.Connection;
using Mentalance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mentalance.Repository
{
    /// <summary>
    /// Implementação do repositório para a entidade Usuario
    /// Responsável apenas pelo acesso direto aos dados no banco
    /// </summary>
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuarioRepository> _logger;

        /// <summary>
        /// Inicializa uma nova instância do UsuarioRepository
        /// </summary>
        /// <param name="context">Contexto do banco de dados</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public UsuarioRepository(AppDbContext context, ILogger<UsuarioRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca todos os usuários cadastrados
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            _logger.LogDebug("Buscando todos os usuários no banco de dados");
            
            try
            {
                var usuarios = await _context.Usuario.ToListAsync();
                _logger.LogDebug("Usuários encontrados: {UsuariosCount}", usuarios.Count());
                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os usuários no banco de dados");
                throw;
            }
        }

        /// <summary>
        /// Busca um usuário pelo ID
        /// </summary>
        public async Task<Usuario?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Buscando usuário pelo ID: {UsuarioId}", id);
            
            try
            {
                var usuario = await _context.Usuario.FindAsync(id);
                
                if (usuario == null)
                {
                    _logger.LogDebug("Usuário não encontrado: {UsuarioId}", id);
                }
                else
                {
                    _logger.LogDebug("Usuário encontrado: {UsuarioId}", usuario.IdUsuario);
                }
                
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário pelo ID: {UsuarioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Busca um usuário pelo e-mail
        /// </summary>
        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            _logger.LogDebug("Buscando usuário pelo email: {Email}", email);
            
            try
            {
                var usuario = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Email == email);
                
                if (usuario == null)
                {
                    _logger.LogDebug("Usuário não encontrado pelo email: {Email}", email);
                }
                else
                {
                    _logger.LogDebug("Usuário encontrado pelo email: {Email}, ID: {UsuarioId}", email, usuario.IdUsuario);
                }
                
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário pelo email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo usuário ao banco de dados
        /// </summary>
        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            _logger.LogDebug("Persistindo novo usuário no banco de dados. Email: {Email}", usuario.Email);
            
            try
            {
                _context.Usuario.Add(usuario);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Usuário persistido com sucesso: {UsuarioId}, Email: {Email}", usuario.IdUsuario, usuario.Email);
                return usuario;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao persistir usuário no banco de dados. Email: {Email}", usuario.Email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar novo usuário no banco de dados. Email: {Email}", usuario.Email);
                throw;
            }
        }


        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        public async Task UpdateAsync(Usuario usuario)
        {
            _logger.LogDebug("Atualizando usuário no banco de dados. ID: {UsuarioId}, Email: {Email}", usuario.IdUsuario, usuario.Email);
            
            try
            {
                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogDebug("Usuário atualizado com sucesso: {UsuarioId}", usuario.IdUsuario);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário no banco de dados. ID: {UsuarioId}", usuario.IdUsuario);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário no banco de dados. ID: {UsuarioId}", usuario.IdUsuario);
                throw;
            }
        }


        /// <summary>
        /// Remove um usuário do banco de dados pelo ID
        /// </summary>
        /// <returns>True se o usuário foi removido, False se não foi encontrado</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogDebug("Iniciando exclusão de usuário. ID: {UsuarioId}", id);
            
            try
            {
                // Busca o usuário pelo ID
                var usuario = await GetByIdAsync(id);

                // Se não encontrou, retorna false
                if (usuario == null)
                {
                    _logger.LogWarning("Usuário não encontrado para exclusão: {UsuarioId}", id);
                    return false;
                }

                // Remove o usuário encontrado
                _context.Usuario.Remove(usuario);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Usuário removido com sucesso: {UsuarioId}", id);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao remover usuário no banco de dados. ID: {UsuarioId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover usuário. ID: {UsuarioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se um usuário existe pelo ID
        /// </summary>
        public async Task<bool> ExisteAsync(int id)
        {
            _logger.LogDebug("Verificando se usuário existe pelo ID: {UsuarioId}", id);
            
            try
            {
                var usuario = await _context.Usuario
                    .Where(e => e.IdUsuario == id)
                    .Select(e => e.IdUsuario)
                    .FirstOrDefaultAsync();
                
                var existe = usuario > 0;
                _logger.LogDebug("Usuário {UsuarioId} existe: {Existe}", id, existe);
                return existe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se usuário existe pelo ID: {UsuarioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se já existe um usuário com o e-mail informado
        /// </summary>
        public async Task<bool> EmailExisteAsync(string email)
        {
            _logger.LogDebug("Verificando se email existe: {Email}", email);
            
            try
            {
                var count = await _context.Usuario
                    .Where(u => u.Email == email)
                    .CountAsync();
                
                var existe = count > 0;
                _logger.LogDebug("Email {Email} existe: {Existe}", email, existe);
                return existe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se email existe: {Email}", email);
                throw;
            }
        }
    }
}