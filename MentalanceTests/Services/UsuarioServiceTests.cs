using Xunit;
using Moq;
using FluentAssertions;
using Mentalance.Repository;
using Mentalance.Service;
using Mentalance.Models;
using Mentalance.Dto;
using Microsoft.Extensions.Logging;

namespace MentalanceTests.Services;

/// <summary>
/// Testes UNITÁRIOS para UsuarioService
/// </summary>
public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepository;
    private readonly Mock<ILogger<UsuarioService>> _mockLogger;
    private readonly UsuarioService _service;


    public UsuarioServiceTests()
    {
        _mockRepository = new Mock<IUsuarioRepository>();
        _mockLogger = new Mock<ILogger<UsuarioService>>();
        _service = new UsuarioService(_mockRepository.Object, _mockLogger.Object);

    }

    #region CreateAsync - Testes de Criação

    /// <summary>
    /// TESTE: Cadastrar usuario com dados válidos
    /// </summary>
    [Fact]
    public async Task CreateAsync_DeveSalvarUsuario_QuandoValido()
    {
        var usuarioDto = new UsuarioDto
        {
            Nome = "João Silva",
            Email = "joao@test.com",
            Senha = "123456",
            Cargo = "Gerente"
        }
        ;

        _mockRepository
            .Setup(r => r.EmailExisteAsync("joao@test.com"))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.AddAsync(It.Is<Usuario>(u =>
            u.Nome == usuarioDto.Nome &&
            u.Email == usuarioDto.Email &&
            u.Senha == usuarioDto.Senha &&
            u.Cargo == usuarioDto.Cargo &&
            u.DataCadastro != default(DateTime)
            )))
            .ReturnsAsync((Usuario u) => u);

        var result = await _service.CreateAsync(usuarioDto);

        result.Should().NotBeNull();

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Once);
    }




    /// <summary>
    /// TESTE: Listar todos os usuarios quando existem
    /// </summary>
    [Fact]
    public async Task GetAllAsync_DeveRetornarTodosUsuarios_QuandoExistirem()
    {

        var usuario = new List<Usuario>
        {
            new Usuario {IdUsuario = 1, Nome = "João Silva",Email = "joao@test.com",Senha = "123456",Cargo = "Gerente",DataCadastro = DateTime.Now },
            new Usuario {IdUsuario = 2, Nome = "Maria Silva",Email = "maria@test.com",Senha = "123456",Cargo = "Gerente",DataCadastro = DateTime.Now }
        }
        ;

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(usuario);

        var result = await _service.GetAllAsync();

        // ASSERT
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Verifica se tem 2 itens
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once); // Verifica se foi chamado
    }


    /// <summary>
    /// TESTE: Buscar usuario por ID válido
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_DeveRetornarUsuario_QuandoIdValido()
    {

        var usuario = new Usuario
        {
            IdUsuario = 1,
            Nome = "João Silva",
            Email = "joao@test.com",
            Senha = "123456",
            Cargo = "Gerente",
            DataCadastro = DateTime.Now
        }
        ;

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

        // ACT
        var result = await _service.GetByIdAsync(1);

        // ASSERT
        result.Should().NotBeNull();
        result!.IdUsuario.Should().Be(1);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    /// <summary>
    /// TESTE: Buscar usuario com ID inválido (deve retornar null)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoIdInvalido()
    {
        var result = await _service.GetByIdAsync(0);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }



    /// <summary>
    /// TESTE: Atualizar usuario com dados válidos
    /// </summary>
    [Fact]
    public async Task UpdateAsync_DeveAtualizaUsuario_QuandoValido()
    {
        // ARRANGE
        var existingUsuario = new Usuario
        {
            IdUsuario = 1,
            Nome = "João Silva",
            Email = "joao@test.com",
            Senha = "123456",
            Cargo = "Gerente",
            DataCadastro = DateTime.Now
        };

        var updatedUsuarioDto = new UsuarioDto
        {
            Nome = "João Morais",
            Email = "joao@test.com",
            Senha = "123456",
            Cargo = "Gerente de Testes",
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUsuario);
        _mockRepository.Setup(r => r.UpdateAsync(It.Is<Usuario>( u =>
            u.Nome == updatedUsuarioDto.Nome &&
            u.Email == updatedUsuarioDto.Email &&
            u.Senha == updatedUsuarioDto.Senha &&
            u.Cargo == updatedUsuarioDto.Cargo &&
            u.DataCadastro != default(DateTime)
            )))
            .Returns(Task.CompletedTask);

        // ACT
        await _service.UpdateAsync(1, updatedUsuarioDto);

        // ASSERT: Verifica se os métodos foram chamados
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Once);
    }

    /// <summary>
    /// TESTE: Tentar atualizar usuario que não existe (deve lançar exceção)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoUsuarioNaoExiste()
    {
        // ARRANGE
        var usuarioDto = new UsuarioDto
        {
            Nome = "João Morais",
            Email = "joao@test.com",
            Senha = "123456",
            Cargo = "Gerente",
        };

        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Usuario?)null);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(999, usuarioDto));

        exception.Message.Should().Contain("999");
    }

    #endregion

    #region DeleteAsync - Testes de Exclusão

    /// <summary>
    /// TESTE: Deletar usuario com ID válido
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeveRemoverUsuario_QuandoIdValido()
    {
        // ARRANGE
        var usuario = new Usuario
        {
            IdUsuario = 1,
            Nome = "João Morais",
            Email = "joao@test.com",
            Senha = "123456",
            Cargo = "Gerente"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // ACT
        await _service.DeleteAsync(1);

        // ASSERT
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    /// <summary>
    /// TESTE: Tentar deletar usuario com ID inválido (deve lançar exceção)
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeveLancarExcecao_QuandoIdInvalido()
    {
        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DeleteAsync(0));

        exception.ParamName.Should().Be("id");
    }

    #endregion

}