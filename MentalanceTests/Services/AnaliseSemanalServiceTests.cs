using FluentAssertions;
using Mentalance;
using Mentalance.API.Models;
using Mentalance.ML.Data;
using Mentalance.ML.Service;
using Mentalance.Models;
using Mentalance.Repository;
using Mentalance.Service;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace MentalanceTests.Services;

/// <summary>
/// Testes UNITÁRIOS para AnaliseSemanalService
/// </summary>
public class AnaliseSemanalServiceTests
{
    private readonly Mock<IAnaliseSemanalRepository> _mockRepository;
    private readonly Mock<ICheckinRepository> _mockCheckinRepository;
    private readonly Mock<IMLService> _mockMLService;
    private readonly Mock<ILogger<AnaliseSemanalService>> _mockLogger;
    private readonly AnaliseSemanalService _service;

    public AnaliseSemanalServiceTests()
    {
        _mockRepository = new Mock<IAnaliseSemanalRepository>();
        _mockCheckinRepository = new Mock<ICheckinRepository>();
        _mockMLService = new Mock<IMLService>();
        _mockLogger = new Mock<ILogger<AnaliseSemanalService>>();
        _service = new AnaliseSemanalService(_mockRepository.Object, _mockCheckinRepository.Object, _mockMLService.Object, _mockLogger.Object);
    }

    #region CreateAsync - Testes de Criação

    /// <summary>
    /// TESTE: Criar analise semanal com dados válidos
    /// </summary>
    [Fact]
    public async Task CreateAsync_DeveSalvarAnaliseSemanal_QuandoValido()
    {
        var analiseSemanal = new AnaliseSemanal
        {
            IdUsuario = 2,
            SemanaReferencia = "Semana 20/11/2025 a 26/11/2025",
            EmocaoPredominante = "Feliz",
            Resumo = "Estou bem hoje",
            Recomendacao = "Continue assim!"
        };

        _mockCheckinRepository
            .Setup(r => r.GetByUsuarioEPeriodoAsync(2))
            .ReturnsAsync(new List<Checkin>
            {
                new Checkin { IdCheckin = 1, IdUsuario = 2, Emoção = EmocaoEnum.Feliz, Texto = "Estou bem hoje", AnáliseSentimento = "positivo", RespostaGerada = "Parabéns! Está se sentindo bem hoje. Continue assim!", DataCheckin = DateTime.Now },
                new Checkin { IdCheckin = 2, IdUsuario = 2, Emoção = EmocaoEnum.Cansado, Texto = "Estou cansado hoje", AnáliseSentimento = "negativo", RespostaGerada = "Está cansado. Tente se relaxar e faça algo que te traga tranquilidade.", DataCheckin = DateTime.Now },
                new Checkin { IdCheckin = 3, IdUsuario = 2, Emoção = EmocaoEnum.Ansioso, Texto = "Estou ansioso hoje", AnáliseSentimento = "negativo", RespostaGerada = "Está ansioso. Tente se relaxar e faça algo que te traga alegria.", DataCheckin = DateTime.Now },
                new Checkin { IdCheckin = 4, IdUsuario = 2, Emoção = EmocaoEnum.Estressado, Texto = "Estou estressado hoje", AnáliseSentimento = "negativo", RespostaGerada = "Está estressao. Tente fazer algo que te traga tranquilidade.", DataCheckin = DateTime.Now }
            });

        _mockMLService
            .Setup(r => r.GerarResumoAsync(2))
            .ReturnsAsync(new AnaliseML
            {
                Resumo = "Estou bem hoje",
                Recomendacao = "Continue assim!"
            });

        _mockRepository.Setup(r => r.AddAsync(It.Is<AnaliseSemanal>(analiseSemanal => analiseSemanal == analiseSemanal)))
        .ReturnsAsync(analiseSemanal);

        var result = await _service.GerarAnaliseSemanalAsync(2);


        result.Should().NotBeNull();
        result.IdUsuario.Should().Be(2);
        result.SemanaReferencia.Should().Be("Semana 20/11/2025 a 26/11/2025");
        result.EmocaoPredominante.Should().Be("Feliz");
        result.Resumo.Should().Be("Estou bem hoje");
        result.Recomendacao.Should().Be("Continue assim!");
    }
    /// <summary>
    /// TESTE: Listar todas as analises semanais quando existem
    /// </summary>
    [Fact]
    public async Task GetAllAsync_DeveRetornarTodosAnaliseSemanal_QuandoExistirem()
    {
        var analiseSemanal = new List<AnaliseSemanal>
        {
            new AnaliseSemanal { IdUsuario = 1, SemanaReferencia = "Semana 1", EmocaoPredominante = "Feliz", Resumo = "Estou bem hoje", Recomendacao = "Continue assim!" },
            new AnaliseSemanal { IdUsuario = 2, SemanaReferencia = "Semana 2", EmocaoPredominante = "Cansado", Resumo = "Estou cansado hoje", Recomendacao = "Tente se relaxar e faça algo que te traga tranquilidade." }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(analiseSemanal);

        // ACT
        var result = await _service.GetAllAsync();

        // ASSERT

        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Verifica se tem 2 itens
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once); // Verifica se foi chamado
    }


    /// <summary>
    /// TESTE: Buscar analise semanal por ID válido
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_DeveRetornarAnaliseSemanal_QuandoIdValido()
    {
        // ARRANGE
        var analiseSemanal = new AnaliseSemanal
        {
            IdUsuario = 1,
            SemanaReferencia = "Semana 1",
            EmocaoPredominante = "Feliz",
            Resumo = "Estou bem hoje",
            Recomendacao = "Continue assim!"
        };

        _mockCheckinRepository
            .Setup(r => r.AddAsync(It.Is<Checkin>(c =>
                c.IdUsuario == 1 &&
                c.Emoção == EmocaoEnum.Feliz &&
                c.Texto == "estou bem e feliz" &&
                !string.IsNullOrEmpty(c.AnáliseSentimento) &&
                !string.IsNullOrEmpty(c.RespostaGerada) &&
                c.DataCheckin != default(DateTime)
            )))
            .ReturnsAsync((Checkin c) => c);

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(analiseSemanal);

        // ACT
        var result = await _service.GetByIdAsync(1);
        

        // ASSERT
        result.Should().NotBeNull();
        result!.IdUsuario.Should().Be(1);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    /// <summary>
    /// TESTE: Buscar analise semanal com ID inválido (deve retornar null)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoIdInvalido()
    {
        var result = await _service.GetByIdAsync(0);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }


    #endregion

    #region DeleteAsync - Testes de Exclusão

    /// <summary>
    /// TESTE: Deletar analise semanal com ID válido
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeveRemoverAnaliseSemanal_QuandoIdValido()
    {
        // ARRANGE
        var analiseSemanal = new AnaliseSemanal
        {
            IdUsuario = 1,
            SemanaReferencia = "Semana 1",
            EmocaoPredominante = "Feliz",
            Resumo = "Estou bem hoje",
            Recomendacao = "Continue assim!"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(analiseSemanal);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // ACT
        await _service.DeleteAsync(1);

        // ASSERT
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    /// <summary>
    /// TESTE: Tentar deletar analise semanal com ID inválido (deve lançar exceção)
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