using Mentalance.Dto;
using Mentalance.Models;
using Mentalance.Repository;
using Mentalance.Service;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace Mentalance.Service
{
    /// <summary>
    /// Implementação do serviço para a entidade Usuario
    /// Contém a lógica de negócio e validações
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private static readonly ActivitySource ActivitySource = new("Mentalance.UsuarioService");
        private readonly IUsuarioRepository _repository;
        private readonly ILogger<UsuarioService> _logger;

        /// <summary>
        /// Inicializa uma nova instância do UsuarioService
        /// </summary>
        /// <param name="repository">Repositório de usuários</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public UsuarioService(IUsuarioRepository repository, ILogger<UsuarioService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Retorna todos os usuários cadastrados
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            using var activity = ActivitySource.StartActivity("GetAllUsuarios");
            
            _logger.LogInformation("Iniciando busca de todos os usuários");
            try {
                activity?.AddEvent(new ActivityEvent("Buscando usuários no repositório"));
                var usuarios = await _repository.GetAllAsync();
                
                var count = usuarios.Count();
                activity?.SetTag("usuario.count", count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Usuários encontrados: {UsuariosCount}", count);
                return usuarios;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao buscar todos os usuários");
                throw;
            }
        }

        /// <summary>
        /// Busca um usuário pelo ID
        /// </summary>
        public async Task<Usuario?> GetByIdAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("GetUsuarioById");
            activity?.SetTag("usuario.id", id);
            
            _logger.LogInformation("Iniciando busca de usuário com ID: {UserId}", id);
            try {
                if (id <= 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "ID inválido");
                    return null;
                }

                activity?.AddEvent(new ActivityEvent("Buscando usuário no repositório"));
                var usuario = await _repository.GetByIdAsync(id);
                
                if (usuario != null)
                {
                    activity?.SetTag("usuario.email", usuario.Email);
                    activity?.SetTag("usuario.cargo", usuario.Cargo);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Usuário não encontrado");
                }
                
                _logger.LogInformation("Usuário encontrado: {UsuarioId}", usuario?.IdUsuario);
                return usuario;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao buscar usuário com ID: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Cria um novo usuário após validar os dados
        /// </summary>
        public async Task<Usuario> CreateAsync(UsuarioDto usuarioDto)
        {
            using var activity = ActivitySource.StartActivity("CreateUsuario");
            
            _logger.LogInformation("Iniciando criação de usuário");
            try {
                // Validação: verifica se o usuário não é nulo
                activity?.AddEvent(new ActivityEvent("Validando dados do usuário"));
                if (usuarioDto == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "UsuarioDto é nulo");
                    _logger.LogWarning("Tentativa de criar usuário com DTO nulo");
                    throw new ArgumentNullException(nameof(usuarioDto), "Usuário não pode ser nulo");
                }

                // Adicionar tags ao trace
                activity?.SetTag("usuario.email", usuarioDto.Email);
                activity?.SetTag("usuario.cargo", usuarioDto.Cargo);

                // Validação: verifica se o e-mail já está cadastrado
                activity?.AddEvent(new ActivityEvent("Verificando se e-mail já existe"));
                var emailExists = await _repository.EmailExisteAsync(usuarioDto.Email);
                if (emailExists)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "E-mail já cadastrado");
                    _logger.LogWarning("Tentativa de criar usuário com e-mail já cadastrado: {Email}", usuarioDto.Email);
                    throw new InvalidOperationException($"Já existe um usuário cadastrado com o e-mail: {usuarioDto.Email}");
                }

                // Validação: verifica se campos obrigatórios estão preenchidos
                if (string.IsNullOrWhiteSpace(usuarioDto.Nome))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Nome nulo");
                    _logger.LogWarning("Tentativa de criar usuário com nome nulo");
                    throw new ArgumentException("Nome do usuário é obrigatório", nameof(usuarioDto.Nome));
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.Email))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "E-mail nulo");
                    _logger.LogWarning("Tentativa de criar usuário com e-mail nulo");
                    throw new ArgumentException("E-mail do usuário é obrigatório", nameof(usuarioDto.Email));
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.Senha))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Senha nula");
                    _logger.LogWarning("Tentativa de criar usuário com senha nula");
                    throw new ArgumentException("Senha do usuário é obrigatória", nameof(usuarioDto.Senha));
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.Cargo))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Cargo nulo");
                    _logger.LogWarning("Tentativa de criar usuário com cargo nulo");
                    throw new ArgumentException("Cargo do usuário é obrigatório", nameof(usuarioDto.Cargo));
                }

                activity?.AddEvent(new ActivityEvent("Criando entidade usuário"));
                Usuario usuario = new Usuario();
                usuario.Nome = usuarioDto.Nome;
                usuario.Email = usuarioDto.Email;
                usuario.Senha = usuarioDto.Senha;
                usuario.Cargo = usuarioDto.Cargo;
                usuario.DataCadastro = DateTime.Now;


                // Adiciona o usuário ao banco
                activity?.AddEvent(new ActivityEvent("Persistindo usuário no banco"));
                var resultado = await _repository.AddAsync(usuario);
                
                // Marcar trace como sucesso
                activity?.SetTag("usuario.id", resultado.IdUsuario);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Usuário {UsuarioId} criado com sucesso", resultado.IdUsuario);
                return resultado;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao criar usuário");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        public async Task UpdateAsync(int id, UsuarioDto usuarioDto)
        {
            using var activity = ActivitySource.StartActivity("UpdateUsuario");
            activity?.SetTag("usuario.id", id);
            
            _logger.LogInformation("Iniciando atualização de usuário com ID: {UserId}", id);
            try {
                // Validação: verifica se o usuário existe
                activity?.AddEvent(new ActivityEvent("Buscando usuário existente"));
                var existingUsuario = await _repository.GetByIdAsync(id);
                if (existingUsuario == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Usuário não encontrado");
                    _logger.LogWarning("Tentativa de atualizar usuário com ID inexistente: {UserId}", id);
                    throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");
                }

                // Validação: verifica se o usuário não é nulo
                activity?.AddEvent(new ActivityEvent("Validando dados do usuário"));
                if (usuarioDto == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "UsuarioDto é nulo");
                    _logger.LogWarning("Tentativa de atualizar usuário com DTO nulo");
                    throw new ArgumentNullException(nameof(usuarioDto), "Usuário não pode ser nulo");
                }

                // Adicionar tags ao trace
                activity?.SetTag("usuario.email", usuarioDto.Email);
                activity?.SetTag("usuario.cargo", usuarioDto.Cargo);

                // Validação: se o e-mail foi alterado, verifica se não está em uso por outro usuário
                if (existingUsuario.Email != usuarioDto.Email)
                {
                    activity?.AddEvent(new ActivityEvent("Verificando se novo e-mail já existe"));
                    var emailExists = await _repository.EmailExisteAsync(usuarioDto.Email);
                    if (emailExists)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "E-mail já cadastrado");
                        _logger.LogWarning("Tentativa de atualizar usuário com e-mail já cadastrado: {Email}", usuarioDto.Email);
                        throw new InvalidOperationException($"Já existe um usuário cadastrado com o e-mail: {usuarioDto.Email}");
                    } else {
                        _logger.LogInformation("E-mail {Email} não está em uso por outro usuário", usuarioDto.Email);
                    }
                }

                // Validação: verifica campos obrigatórios
                if (string.IsNullOrWhiteSpace(usuarioDto.Nome))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Nome nulo");
                    _logger.LogWarning("Tentativa de atualizar usuário com nome nulo");
                    throw new ArgumentException("Nome do usuário é obrigatório", nameof(usuarioDto.Nome));
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.Email))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "E-mail nulo");
                    _logger.LogWarning("Tentativa de atualizar usuário com e-mail nulo");
                    throw new ArgumentException("E-mail do usuário é obrigatório", nameof(usuarioDto.Email));
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.Senha))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Senha nula");
                    _logger.LogWarning("Tentativa de atualizar usuário com senha nula");
                    throw new ArgumentException("Senha do usuário é obrigatória", nameof(usuarioDto.Senha));
                }

                if (string.IsNullOrWhiteSpace(usuarioDto.Cargo))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Cargo nulo");
                    _logger.LogWarning("Tentativa de atualizar usuário com cargo nulo");
                    throw new ArgumentException("Cargo do usuário é obrigatório", nameof(usuarioDto.Cargo));
                }

                // Atualiza o usuário existente com os novos dados
                activity?.AddEvent(new ActivityEvent("Atualizando entidade usuário"));
                existingUsuario.Nome = usuarioDto.Nome;
                existingUsuario.Email = usuarioDto.Email;
                existingUsuario.Senha = usuarioDto.Senha;
                existingUsuario.Cargo = usuarioDto.Cargo;
                existingUsuario.DataCadastro = DateTime.Now;


                // Salva as alterações
                activity?.AddEvent(new ActivityEvent("Persistindo atualização no banco"));
                await _repository.UpdateAsync(existingUsuario);

                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Usuário {UsuarioId} atualizado com sucesso", existingUsuario.IdUsuario);
                
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao atualizar usuário com ID: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Remove um usuário do sistema
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("DeleteUsuario");
            activity?.SetTag("usuario.id", id);
            
            _logger.LogInformation("Iniciando exclusão de usuário com ID: {UserId}", id);
            try {
                // Validação: verifica se o ID é válido
                if (id <= 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "ID inválido");
                    throw new ArgumentException("ID inválido", nameof(id));
                }

                // Busca o usuário
                activity?.AddEvent(new ActivityEvent("Buscando usuário para exclusão"));
                var usuario = await _repository.GetByIdAsync(id);
                if (usuario == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Usuário não encontrado");
                    throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");
                }

                // Adicionar tags do usuário antes de excluir
                activity?.SetTag("usuario.email", usuario.Email);
                activity?.SetTag("usuario.cargo", usuario.Cargo);

                // Remove o usuário
                activity?.AddEvent(new ActivityEvent("Excluindo usuário do banco"));
                await _repository.DeleteAsync(id);

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao excluir usuário com ID: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Realiza o login de um usuário
        /// </summary>
        public async Task<Usuario?> LoginAsync(LoginDto loginDto)
        {
            using var activity = ActivitySource.StartActivity("LoginUsuario");
            
            _logger.LogInformation("Iniciando login de usuário");
            try {
                // Validação: verifica se os dados de login foram fornecidos
                activity?.AddEvent(new ActivityEvent("Validando dados de login"));
                if (loginDto == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "LoginDto é nulo");
                    throw new ArgumentNullException(nameof(loginDto), "Dados de login não podem ser nulos");
                }

                if (string.IsNullOrWhiteSpace(loginDto.Email))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "E-mail nulo");
                    throw new ArgumentException("E-mail é obrigatório", nameof(loginDto.Email));
                }

                if (string.IsNullOrWhiteSpace(loginDto.Senha))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Senha nula");
                    throw new ArgumentException("Senha é obrigatória", nameof(loginDto.Senha));
                }

                // Adicionar tag do e-mail (sem senha por segurança)
                activity?.SetTag("usuario.email", loginDto.Email);

                // Busca o usuário pelo e-mail
                activity?.AddEvent(new ActivityEvent("Buscando usuário por e-mail"));
                var usuario = await _repository.GetByEmailAsync(loginDto.Email);

                // Verifica se o usuário existe e se a senha confere
                activity?.AddEvent(new ActivityEvent("Validando credenciais"));
                if (usuario == null || usuario.Senha != loginDto.Senha)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Credenciais inválidas");
                    activity?.SetTag("login.sucesso", false);
                    _logger.LogWarning("Tentativa de login com credenciais inválidas para: {Email}", loginDto.Email);
                    return null; // Credenciais inválidas
                }

                activity?.SetTag("usuario.id", usuario.IdUsuario);
                activity?.SetTag("login.sucesso", true);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Login de usuário {Email} bem-sucedido", loginDto.Email);
                return usuario; // Login bem-sucedido
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao fazer login");
                throw;
            }
        }
    }
}