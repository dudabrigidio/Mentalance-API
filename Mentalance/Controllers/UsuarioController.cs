using Mentalance.Dto;
using Mentalance.Models;
using Mentalance.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Mentalance.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar os usuários do sistema e autenticação
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;
        private readonly ILogger<UsuariosController> _logger;

        /// <summary>
        /// Inicializa uma nova instância do UsuariosController
        /// </summary>
        /// <param name="service">Serviço de usuários</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public UsuariosController(IUsuarioService service, ILogger<UsuariosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lista de todos os usuários cadastrados
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario()
        {
            _logger.LogInformation("Iniciando busca de todos os usuários");

            try
            {
                var usuarios = await _service.GetAllAsync();
                _logger.LogInformation("Usuários encontrados: {UsuariosCount}", usuarios.Count());
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os usuários");
                throw;
            }
        }

        /// <summary>
        /// Busca de um usuário pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            _logger.LogInformation("Iniciando busca de usuário com ID: {UsuarioId}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar usuário com ID inválido: {UsuarioId}", id);
                return NotFound();
            }

            try
            {
                var usuario = await _service.GetByIdAsync(id);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuário não encontrado com ID: {UsuarioId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Usuário encontrado: {UsuarioId}", usuario.IdUsuario);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário com ID: {UsuarioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Cadastro de um usuário
        /// </summary>      
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST/api/Usuario
        ///        {
        ///             "nome": "Maria Silva",
        ///             "email": "maria@empresa.com",
        ///             "senha" : "123silva",
        ///             "cargo": "Gerente de Marketing"
        ///         }
        ///       
        /// </remarks>
        /// <param name="usuarioDto">Dados do usuário para criação</param>
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(UsuarioDto usuarioDto)
        {
            _logger.LogInformation("Iniciando criação de usuário. Email: {Email}", usuarioDto?.Email);

            try
            {
                var novoUsuario = await _service.CreateAsync(usuarioDto);
                _logger.LogInformation("Usuário criado com sucesso: {UsuarioId}, Email: {Email}", novoUsuario.IdUsuario, novoUsuario.Email);
                return CreatedAtAction("GetUsuario", new { id = novoUsuario.IdUsuario }, novoUsuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário. Email: {Email}", usuarioDto?.Email);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário. Email: {Email}", usuarioDto?.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário. Email: {Email}", usuarioDto?.Email);
                throw;
            }
        }

        /// <summary>
        /// Alteração de um usuário já cadastrado
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     PUT/api/Usuario
        ///        {
        ///             "nome": "Maria Silva",
        ///             "email": "maria@empresa.com",
        ///             "senha" : "123silva",
        ///             "cargo": "Gerente de Marketing"
        ///         }
        ///       
        /// </remarks>
        /// <param name="id">ID do usuário</param>
        /// <param name="usuarioDto">Dados do usuário para atualização</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, UsuarioDto usuarioDto)
        {
            _logger.LogInformation("Iniciando atualização de usuário com ID: {UsuarioId}", id);

            try
            {
                await _service.UpdateAsync(id, usuarioDto);
                _logger.LogInformation("Usuário atualizado com sucesso: {UsuarioId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogError("Erro ao atualizar usuário com ID: {UsuarioId} - Usuário não encontrado", id);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário com ID: {UsuarioId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário com ID: {UsuarioId}", id);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário com ID: {UsuarioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Apagar um usuário pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            _logger.LogInformation("Iniciando exclusão de usuário com ID: {UsuarioId}", id);

            try
            {
                await _service.DeleteAsync(id);
                _logger.LogInformation("Usuário excluído com sucesso: {UsuarioId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário com ID: {UsuarioId} - Usuário não encontrado", id);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário com ID: {UsuarioId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário com ID: {UsuarioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Realiza login do usuário
        /// </summary>
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            _logger.LogInformation("Iniciando tentativa de login. Email: {Email}", loginDto?.Email);

            try
            {
                var usuario = await _service.LoginAsync(loginDto);

                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de login falhou. Email ou senha inválidos. Email: {Email}", loginDto?.Email);
                    return Unauthorized(new { message = "Email ou senha inválidos" });
                }

                var idToken = Guid.NewGuid().ToString();
                _logger.LogInformation("Login bem-sucedido. Usuário: {UsuarioId}, Email: {Email}", usuario.IdUsuario, usuario.Email);
                return Ok(new { message = "Login bem-sucedido", idToken = idToken, IdUsuario = usuario.IdUsuario });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro ao realizar login. Email: {Email}", loginDto?.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar login. Email: {Email}", loginDto?.Email);
                throw;
            }
        }
    }
}