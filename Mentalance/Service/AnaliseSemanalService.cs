using Mentalance.ML.Service;
using Mentalance.Models;
using Mentalance.Repository;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace Mentalance.Service
{
    /// <summary>
    /// Implementação do serviço para a entidade AnaliseSemanal
    /// Contém a lógica de negócio e validações
    /// </summary>
    public class AnaliseSemanalService : IAnaliseSemanalService
    {
        private static readonly ActivitySource ActivitySource = new("Mentalance.AnaliseSemanalService");
        private readonly IAnaliseSemanalRepository _repository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly IMLService _mlService;
        private readonly ILogger<AnaliseSemanalService> _logger;

        /// <summary>
        /// Inicializa uma nova instância do AnaliseSemanalService
        /// </summary>
        /// <param name="repository">Repositório de análises semanais</param>
        /// <param name="checkinRepository">Repositório de check-ins</param>
        /// <param name="mlService">Serviço de machine learning</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public AnaliseSemanalService(
            IAnaliseSemanalRepository repository,
            ICheckinRepository checkinRepository,
            IMLService mlService,
            ILogger<AnaliseSemanalService> logger)
        {
            _repository = repository;
            _checkinRepository = checkinRepository;
            _mlService = mlService;
            _logger = logger;
        }


        /// <summary>
        /// Retorna todas as análises realizadas
        /// </summary>
        public async Task<IEnumerable<AnaliseSemanal>> GetAllAsync()
        {
            using var activity = ActivitySource.StartActivity("GetAllAnalisesSemanais");
            
            _logger.LogInformation("Iniciando busca de todas as análises semanales");
            try {
                activity?.AddEvent(new ActivityEvent("Buscando análises semanais no repositório"));
                var analises = await _repository.GetAllAsync();
                
                var count = analises.Count();
                activity?.SetTag("analise.count", count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Análises semanales encontradas: {AnalisesCount}", count);
                return analises;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao buscar todas as análises semanales");
                throw;
            }
        }

        /// <summary>
        /// Busca análise semanal por ID
        /// </summary>
        public async Task<AnaliseSemanal?> GetByIdAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("GetAnaliseSemanalById");
            activity?.SetTag("analise.id", id);
            
            _logger.LogInformation("Iniciando busca de análise semanal com ID: {AnaliseId}", id);
            try {
                if (id <= 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "ID inválido");
                    return null;
                }

                activity?.AddEvent(new ActivityEvent("Buscando análise semanal no repositório"));
                var analise = await _repository.GetByIdAsync(id);
                
                if (analise != null)
                {
                    activity?.SetTag("analise.usuario_id", analise.IdUsuario);
                    activity?.SetTag("analise.emocao_predominante", analise.EmocaoPredominante);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Análise não encontrada");
                }
                
                _logger.LogInformation("Análise semanal encontrada: {AnaliseId}", analise?.IdAnalise);
                return analise;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao buscar análise semanal com ID: {AnaliseId}", id);
                throw;
            }
        }


        /// <summary>
        /// Gera uma análise semanal usando ML.NET baseada nos últimos 7 dias
        /// </summary>
        public async Task<AnaliseSemanal> GerarAnaliseSemanalAsync(int idUsuario)
        {
            using var activity = ActivitySource.StartActivity("GerarAnaliseSemanal");
            activity?.SetTag("analise.usuario_id", idUsuario);
            
            _logger.LogInformation("Iniciando geração de análise semanal para usuário: {UserId}", idUsuario);
            try {
                // Busca checkins dos últimos 7 dias
                activity?.AddEvent(new ActivityEvent("Buscando checkins dos últimos 7 dias"));
                var checkins = await _checkinRepository.GetByUsuarioEPeriodoAsync(idUsuario);
                var checkinsList = checkins.ToList();

                activity?.SetTag("analise.checkins_count", checkinsList.Count);

                if (!checkinsList.Any())
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Não há check-ins nos últimos 7 dias");
                    _logger.LogWarning("Não há check-ins nos últimos 7 dias para gerar a análise.");
                    throw new InvalidOperationException("Não há check-ins nos últimos 7 dias para gerar a análise.");
                }


                // Calcula período da semana
                activity?.AddEvent(new ActivityEvent("Calculando período da semana"));
                var dataFim = DateTime.Now;
                var dataInicio = dataFim.AddDays(-7);
                var semanaReferencia = $"Semana {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}";
                activity?.SetTag("analise.semana_referencia", semanaReferencia);

                // Calcula emoção predominante
                activity?.AddEvent(new ActivityEvent("Calculando emoção predominante"));
                var emocaoPredominante = CalcularEmocaoPredominante(checkinsList);
                activity?.SetTag("analise.emocao_predominante", emocaoPredominante);

                // Usa MLService para gerar o resumo
                activity?.AddEvent(new ActivityEvent("Gerando resumo com MLService"));
                var analiseML = await _mlService.GerarResumoAsync(idUsuario, checkinsList);

                activity?.AddEvent(new ActivityEvent("Criando entidade análise semanal"));
                var analise = new AnaliseSemanal
                {
                    IdUsuario = idUsuario,
                    SemanaReferencia = semanaReferencia,
                    EmocaoPredominante = emocaoPredominante,
                    Resumo = analiseML.Resumo,
                    Recomendacao = analiseML.Recomendacao,
                };

                // Adiciona a análise ao banco
                activity?.AddEvent(new ActivityEvent("Persistindo análise semanal no banco"));
                var resultado = await _repository.AddAsync(analise);
                
                // Marcar trace como sucesso
                activity?.SetTag("analise.id", resultado.IdAnalise);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Análise semanal {AnaliseId} gerada com sucesso para usuário: {UserId}", resultado.IdAnalise, idUsuario);
                return resultado;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao gerar análise semanal para usuário: {UserId}", idUsuario);
                throw;
            }
        }


        /// <summary>
        /// Remove uma análise do sistema
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("DeleteAnaliseSemanal");
            activity?.SetTag("analise.id", id);
            
            _logger.LogInformation("Iniciando exclusão de análise semanal com ID: {AnaliseId}", id);
            try {
                // Validação: verifica se o ID é válido
                if (id <= 0)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "ID inválido");
                        _logger.LogWarning("Tentativa de excluir análise semanal com ID inválido: {AnaliseId}", id);
                        throw new ArgumentException("ID inválido", nameof(id));
                    }

                // Busca a análise
                activity?.AddEvent(new ActivityEvent("Buscando análise semanal para exclusão"));
                var analise = await _repository.GetByIdAsync(id);
                if (analise == null)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "Análise não encontrada");
                        _logger.LogWarning("Tentativa de excluir análise semanal com ID inexistente: {AnaliseId}", id);
                        throw new KeyNotFoundException($"Análise com ID {id} não encontrada");
                    }

                // Adicionar tags da análise antes de excluir
                activity?.SetTag("analise.usuario_id", analise.IdUsuario);
                activity?.SetTag("analise.emocao_predominante", analise.EmocaoPredominante);

                // Remove a análise
                activity?.AddEvent(new ActivityEvent("Excluindo análise semanal do banco"));
                await _repository.DeleteAsync(id);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Análise semanal {AnaliseId} excluída com sucesso", id);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao excluir análise semanal com ID: {AnaliseId}", id);
                throw;
            }
        }


        /// <summary>
        /// Calcula a emoção predominante em uma lista de checkins
        /// </summary>
        private string CalcularEmocaoPredominante(List<Checkin> checkins)
        {
            if (!checkins.Any())
                return "Calmo";

            var contagemEmocoes = checkins
                .GroupBy(c => c.Emocao.ToString())
                .Select(g => new { Emocao = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Se não há emoções, retorna padrão
            if (!contagemEmocoes.Any())
                return "Calmo";

            var maxCount = contagemEmocoes.First().Count;
            
            // Verifica se há empate (múltiplas emoções com a mesma contagem máxima)
            var emocoesEmpatadas = contagemEmocoes
                .Where(x => x.Count == maxCount)
                .ToList();

            // Se há empate e todas as emoções têm a mesma frequência, retorna "Misto"
            if (emocoesEmpatadas.Count > 1 && emocoesEmpatadas.Count == contagemEmocoes.Count)
            {
                return "Misto";
            }

            // Caso contrário, retorna a emoção com maior frequência
            return contagemEmocoes.First().Emocao;
        }
    }
}
