using Xunit;
using Moq;
using FluentAssertions;
using Mentalance.Service;
using Mentalance.Models;
using Mentalance.Repository;
using Mentalance.API.Models;
using Mentalance;
using Microsoft.Extensions.Logging;

namespace MentalanceTests.Services;

/// <summary>
/// Testes UNITÁRIOS para CheckinService
/// </summary>
public class CheckinServiceTests
{
    private readonly Mock<ICheckinRepository> _mockRepository;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly Mock<ILogger<CheckinService>> _mockLogger;
    private readonly CheckinService _service;

    public CheckinServiceTests()
    {
        _mockRepository = new Mock<ICheckinRepository>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();
        _mockLogger = new Mock<ILogger<CheckinService>>();
        _service = new CheckinService(_mockRepository.Object, _mockUsuarioRepository.Object, _mockLogger.Object);
    }

    #region CreateAsync - Testes de Criação

    /// <summary>
    /// TESTE: Criar checkin com dados válidos
    /// </summary>
    [Fact]
    public async Task CreateAsync_DeveSalvarCheckin_QuandoValido()
    {
        var checkinDto = new CheckinDto
        {
            IdUsuario = 2,
            Emoção = EmocaoEnum.Feliz,
            Texto = "Estou bem hoje"
        };

        _mockUsuarioRepository
            .Setup(r => r.ExisteAsync(2))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(r => r.AddAsync(It.Is<Checkin>(c =>
                c.IdUsuario == checkinDto.IdUsuario &&
                c.Emoção == checkinDto.Emoção && 
                c.Texto == checkinDto.Texto &&
                !string.IsNullOrEmpty(c.AnáliseSentimento) && 
                !string.IsNullOrEmpty(c.RespostaGerada) &&
                c.DataCheckin != default(DateTime)
            )))
            .ReturnsAsync((Checkin c) => c );

        var result = await _service.CreateAsync(checkinDto);

        result.Should().NotBeNull();

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Checkin>()), Times.Once);
    }


    /// <summary>
    /// TESTE: Listar todos os checkins quando existem
    /// </summary>
    [Fact]
    public async Task GetAllAsync_DeveRetornarTodosCheckins_QuandoExistirem()
    {
        var checkins = new List<Checkin>
        {
            new Checkin { IdCheckin = 1, IdUsuario = 1, Emoção = EmocaoEnum.Feliz, Texto = "Estou bem hoje", AnáliseSentimento = "positivo", RespostaGerada = "Parabéns! Está se sentindo bem hoje. Continue assim!", DataCheckin = DateTime.Now },
            new Checkin { IdCheckin = 2, IdUsuario = 2, Emoção = EmocaoEnum.Cansado, Texto = "Estou cansado hoje", AnáliseSentimento = "negativo", RespostaGerada = "Está cansado. Tente se relaxar e faça algo que te traga tranquilidade.", DataCheckin = DateTime.Now }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(checkins);

        // ACT
        var result = await _service.GetAllAsync();

        // ASSERT
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Verifica se tem 2 itens
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once); // Verifica se foi chamado
    }


    /// <summary>
    /// TESTE: Buscar checkin por ID válido
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_DeveRetornarCheckin_QuandoIdValido()
    {
        // ARRANGE
        var checkin = new Checkin
        {
            IdCheckin = 1,
            IdUsuario = 1,
            Emoção = EmocaoEnum.Feliz,
            Texto = "Estou bem hoje",
            AnáliseSentimento = "positivo",
            RespostaGerada = "Parabéns! Está se sentindo bem hoje. Continue assim!",
            DataCheckin = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(checkin);

        // ACT
        var result = await _service.GetByIdAsync(1);

        // ASSERT
        result.Should().NotBeNull();
        result!.IdCheckin.Should().Be(1);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    /// <summary>
    /// TESTE: Buscar checkin com ID inválido (deve retornar null)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoIdInvalido()
    {
        var result = await _service.GetByIdAsync(0);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// TESTE: Atualizar checkin com dados válidos
    /// </summary>
    [Fact]
    public async Task UpdateAsync_DeveAtualizarCheckin_QuandoValido()
    {
        // ARRANGE
        var existingCheckin = new Checkin
        {
            IdCheckin = 1,
            IdUsuario = 1,
            Emoção = EmocaoEnum.Feliz,
            Texto = "Estou bem hoje",
            AnáliseSentimento = "positivo",
            RespostaGerada = "Parabéns! Está se sentindo bem hoje. Continue assim!",
            DataCheckin = DateTime.Now
        };

        var updatedCheckinDto = new CheckinDto
        {
            IdUsuario = 2,
            Emoção = EmocaoEnum.Cansado,
            Texto = "Estou cansado hoje"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingCheckin);


        _mockUsuarioRepository
            .Setup(r => r.ExisteAsync(2))
            .ReturnsAsync(true);

        _mockRepository.Setup(r => r.UpdateAsync(It.Is<Checkin>(c =>
                c.IdUsuario == updatedCheckinDto.IdUsuario &&
                c.Emoção == updatedCheckinDto.Emoção &&
                c.Texto == updatedCheckinDto.Texto &&
                !string.IsNullOrEmpty(c.AnáliseSentimento) &&
                !string.IsNullOrEmpty(c.RespostaGerada) &&
                c.DataCheckin != default(DateTime)
                )))
        .Returns(Task.CompletedTask);

        // ACT
        await _service.UpdateAsync(1, updatedCheckinDto);

        // ASSERT: Verifica se os métodos foram chamados
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Checkin>()), Times.Once);
    }

    /// <summary>
    /// TESTE: Tentar atualizar checkin que não existe (deve lançar exceção)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoCheckinNaoExiste()
    {
        // ARRANGE
        var checkinDto = new CheckinDto
        {
            IdUsuario = 2,
            Emoção = EmocaoEnum.Feliz,
            Texto = "Estou bem hoje"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Checkin?)null);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(999, checkinDto));

        exception.Message.Should().Contain("999"); // Verifica se a mensagem contém o ID
    }

    #endregion

    #region DeleteAsync - Testes de Exclusão

    /// <summary>
    /// TESTE: Deletar checkin com ID válido
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeveRemoverCheckin_QuandoIdValido()
    {
        // ARRANGE
        var checkin = new Checkin
        {
            IdCheckin = 1,
            IdUsuario = 1,
            Emoção = EmocaoEnum.Feliz,
            Texto = "Estou bem hoje",
            AnáliseSentimento = "positivo",
            RespostaGerada = "Parabéns! Está se sentindo bem hoje. Continue assim!",
            DataCheckin = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(checkin);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // ACT
        await _service.DeleteAsync(1);

        // ASSERT
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    /// <summary>
    /// TESTE: Tentar deletar checkin com ID inválido (deve lançar exceção)
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