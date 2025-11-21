using Mentalance.ML.Data;
using Mentalance.Models;
using Mentalance.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System.Diagnostics;
using System.Text.Json;

namespace Mentalance.ML.Service
{
    /// <summary>
    /// Serviço de Machine Learning para geração de análises e recomendações baseadas em check-ins
    /// </summary>
    public class MLService : IMLService
    {
        private static readonly ActivitySource ActivitySource = new("Mentalance.MLService");
        private readonly MLContext _mlContext;
        private readonly ICheckinRepository _checkinRepository;
        private readonly ILogger<MLService> _logger;
        private ITransformer? _model;
        private ITransformer? _modelRecomendacao;

        /// <summary>
        /// Inicializa uma nova instância do MLService
        /// </summary>
        /// <param name="checkinRepository">Repositório de check-ins para buscar dados de treinamento</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public MLService(ICheckinRepository checkinRepository, ILogger<MLService> logger)
        {
            _mlContext = new MLContext();
            _checkinRepository = checkinRepository;
            _logger = logger;

            using var activity = ActivitySource.StartActivity("InicializarMLService");
            activity?.AddEvent(new ActivityEvent("Carregando dados de treinamento"));
            
            try
            {
                var dadosTreino = CarregarDadosTreino().GetAwaiter().GetResult();
                activity?.SetTag("ml.dados_treino_count", dadosTreino.Count);
                
                activity?.AddEvent(new ActivityEvent("Preparando data view"));
                var dataView = _mlContext.Data.LoadFromEnumerable(dadosTreino);

                // Pipeline para modelo de Resumo
                activity?.AddEvent(new ActivityEvent("Criando pipeline do modelo de Resumo"));
                var pipelineResumo = _mlContext.Transforms.Conversion.MapValueToKey(
                        outputColumnName: "Label",
                        inputColumnName: nameof(SemanaAnalise.ResumoEsperado))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("TextoFeatures", nameof(SemanaAnalise.Textos)))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("EmocaoFeatures", nameof(SemanaAnalise.Emocoes)))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("EmocaoPredominanteFeatures", nameof(SemanaAnalise.EmocaoPredominante)))
                    .Append(_mlContext.Transforms.Concatenate("Features", "TextoFeatures", "EmocaoFeatures", "EmocaoPredominanteFeatures"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: "Label",
                        featureColumnName: "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                        outputColumnName: "PredictedLabel"));

                // Treina o modelo de Resumo
                activity?.AddEvent(new ActivityEvent("Treinando modelo de Resumo"));
                _model = pipelineResumo.Fit(dataView);
                activity?.SetTag("ml.modelo_resumo_carregado", _model != null);
                _logger.LogInformation("Modelo de Resumo treinado com sucesso");

                // Pipeline para modelo de Recomendação
                activity?.AddEvent(new ActivityEvent("Criando pipeline do modelo de Recomendação"));
                var pipelineRecomendacao = _mlContext.Transforms.Conversion.MapValueToKey(
                        outputColumnName: "Label",
                        inputColumnName: nameof(SemanaAnalise.RecomendacaoEsperada))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("TextoFeatures", nameof(SemanaAnalise.Textos)))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("EmocaoFeatures", nameof(SemanaAnalise.Emocoes)))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("EmocaoPredominanteFeatures", nameof(SemanaAnalise.EmocaoPredominante)))
                    .Append(_mlContext.Transforms.Concatenate("Features", "TextoFeatures", "EmocaoFeatures", "EmocaoPredominanteFeatures"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: "Label",
                        featureColumnName: "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                        outputColumnName: "PredictedLabel"));

                // Treina o modelo de Recomendação
                activity?.AddEvent(new ActivityEvent("Treinando modelo de Recomendação"));
                _modelRecomendacao = pipelineRecomendacao.Fit(dataView);
                activity?.SetTag("ml.modelo_recomendacao_carregado", _modelRecomendacao != null);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Modelo de Recomendação treinado com sucesso");
                _logger.LogInformation("MLService inicializado com sucesso");
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao inicializar MLService");
                throw;
            }
        }



        /// <summary>
        /// Gera análise/resumo textual com base nos checkins de uma semana (7 dias)
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Predição com resumo textual gerado</returns>
        public async Task<AnaliseML> GerarResumoAsync(int usuarioId)
        {
            using var activity = ActivitySource.StartActivity("GerarResumoML");
            activity?.SetTag("ml.usuario_id", usuarioId);
            activity?.SetTag("ml.modelo", "AnaliseSemanal");
            
            _logger.LogInformation("Iniciando geração de resumo ML para usuário: {UsuarioId}", usuarioId);
            
            try
            {
                // Busca checkins dos últimos 7 dias
                activity?.AddEvent(new ActivityEvent("Buscando checkins dos últimos 7 dias"));
                var checkins = await _checkinRepository.GetByUsuarioEPeriodoAsync(usuarioId);
                var checkinsList = checkins.ToList();
                activity?.SetTag("ml.checkins_count", checkinsList.Count);

                // Validação de entrada
                if (checkinsList.Count == 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Não há checkins para analisar");
                    _logger.LogWarning("Não há checkins para analisar nos últimos 7 dias para usuário: {UsuarioId}", usuarioId);
                    throw new Exception("Não há checkins para analisar nos últimos 7 dias");
                }

                // Agrega os checkins da semana
                activity?.AddEvent(new ActivityEvent("Preparando dados para predição"));
                var emocoes = checkinsList.Select(c => c.Emocao.ToString()).ToList();
                var textos = checkinsList.Select(c => c.Texto).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                var emocaoPredominante = CalcularEmocaoPredominante(checkinsList);
                activity?.SetTag("ml.emocao_predominante", emocaoPredominante);

                var emocoesCombinadas = string.Join(",", emocoes);
                var textosCombinados = string.Join(". ", textos);

                // Cria um exemplo de entrada agregado
                var input = new SemanaAnalise
                {
                    Emocoes = emocoesCombinadas,
                    Textos = textosCombinados,
                    EmocaoPredominante = emocaoPredominante,
                    ResumoEsperado = string.Empty,
                    RecomendacaoEsperada = string.Empty
                };

                var resultado = new AnaliseML();

                // Tenta usar o modelo ML.NET para Resumo
                activity?.AddEvent(new ActivityEvent("Executando predição de Resumo"));
                try
                {
                    if (_model != null)
                    {
                        var engineResumo = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, AnaliseML>(_model);
                        var resultadoResumo = engineResumo.Predict(input);
                        resultado.Resumo = resultadoResumo.Resumo;
                        activity?.SetTag("ml.resumo_gerado_ml", !string.IsNullOrWhiteSpace(resultado.Resumo));
                        _logger.LogInformation("Resumo gerado com sucesso usando modelo ML para usuário: {UsuarioId}", usuarioId);
                    }
                    else
                    {
                        activity?.SetTag("ml.modelo_resumo_disponivel", false);
                        _logger.LogWarning("Modelo de Resumo não está disponível");
                    }
                }
                catch (Exception ex)
                {
                    activity?.SetTag("ml.erro_predicao_resumo", true);
                    _logger.LogWarning(ex, "Erro ao usar modelo ML.NET para resumo, usando fallback");
                }

                // Tenta usar o modelo ML.NET para Recomendação
                activity?.AddEvent(new ActivityEvent("Executando predição de Recomendação"));
                try
                {
                    if (_modelRecomendacao != null)
                    {
                        var engineRecomendacao = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, RecomendacaoML>(_modelRecomendacao);
                        var resultadoRecomendacao = engineRecomendacao.Predict(input);
                        resultado.Recomendacao = resultadoRecomendacao.Recomendacao;
                        activity?.SetTag("ml.recomendacao_gerada_ml", !string.IsNullOrWhiteSpace(resultado.Recomendacao));
                        _logger.LogInformation("Recomendação gerada com sucesso usando modelo ML para usuário: {UsuarioId}", usuarioId);
                    }
                    else
                    {
                        activity?.SetTag("ml.modelo_recomendacao_disponivel", false);
                        _logger.LogWarning("Modelo de Recomendação não está disponível");
                    }
                }
                catch (Exception ex)
                {
                    activity?.SetTag("ml.erro_predicao_recomendacao", true);
                    _logger.LogWarning(ex, "Erro ao usar modelo ML.NET para recomendação, usando fallback");
                }

                // Se o resumo retornado estiver vazio ou for muito genérico, usa fallback
                if (string.IsNullOrWhiteSpace(resultado.Resumo) ||
                    resultado.Resumo == "Período com variações emocionais diversas")
                {
                    activity?.AddEvent(new ActivityEvent("Usando fallback para resumo"));
                    activity?.SetTag("ml.usou_fallback_resumo", true);
                    _logger.LogInformation("Usando fallback para gerar resumo para usuário: {UsuarioId}", usuarioId);
                    var fallback = AnalisarComFallback(emocoes, textos, emocaoPredominante);
                    resultado.Resumo = fallback.Resumo;
                }

                // Se a recomendação estiver vazia, usa fallback
                if (string.IsNullOrWhiteSpace(resultado.Recomendacao))
                {
                    activity?.AddEvent(new ActivityEvent("Usando fallback para recomendação"));
                    activity?.SetTag("ml.usou_fallback_recomendacao", true);
                    _logger.LogInformation("Usando fallback para gerar recomendação para usuário: {UsuarioId}", usuarioId);
                    resultado.Recomendacao = GerarRecomendacao(emocaoPredominante);
                }

                activity?.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation("Resumo ML gerado com sucesso para usuário: {UsuarioId}", usuarioId);
                
                return resultado;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao gerar resumo ML para usuário: {UsuarioId}", usuarioId);
                throw;
            }
        }



        /// <summary>
        /// Calcula a emoção predominante em uma lista de checkins
        /// </summary>
        private string CalcularEmocaoPredominante(List<Checkin> checkins)
        {
            var contagemEmocoes = checkins
                .GroupBy(c => c.Emocao.ToString())
                .Select(g => new { Emocao = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return contagemEmocoes.FirstOrDefault()?.Emocao ?? "Calmo";
        }


        /// <summary>
        /// Analisa usando apenas regras (fallback quando o modelo não está disponível)
        /// Gera resumo e recomendação juntos
        /// </summary>
        private AnaliseML AnalisarComFallback(List<string> emocoes, List<string> textos, string emocaoPredominante)
        {
            var resumo = GerarResumoEsperadoSemanal(emocoes, textos, emocaoPredominante);
            var recomendacao = GerarRecomendacao(emocaoPredominante);
            
            return new AnaliseML
            {
                Resumo = resumo,
                Recomendacao = recomendacao
            };
        }

        /// <summary>
        /// Gera resumo esperado baseado em regras para uma semana agregada (múltiplas emoções e textos)
        /// </summary>
        private string GerarResumoEsperadoSemanal(List<string> emocoes, List<string> textos, string emocaoPredominante)
        {
            // Combina todos os textos em uma única string para análise
            var textosCombinados = string.Join(". ", textos);
            var textoLower = textosCombinados.ToLowerInvariant();
            var emocaoPredominanteLower = emocaoPredominante.ToLowerInvariant();

            // Conta frequência de cada emoção
            var contagemEmocoes = emocoes
                .GroupBy(e => e.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.Count());

            // Gera resumo baseado na emoção predominante e conteúdo dos textos
            var resumo = "";

            // Regras baseadas na emoção FELIZ
            if (emocaoPredominanteLower.Contains("feliz"))
            {
                if (textoLower.Contains("produtivo") || textoLower.Contains("consegui") || textoLower.Contains("finalizei") ||
                    textoLower.Contains("completei") || textoLower.Contains("terminei"))
                    resumo = "Semana positiva com foco em produtividade e realizações";
                else if (textoLower.Contains("família") || textoLower.Contains("amigos") || textoLower.Contains("tempo") ||
                    textoLower.Contains("pessoas") || textoLower.Contains("amor"))
                    resumo = "Período feliz com momentos importantes de conexão pessoal";
                else if (textoLower.Contains("sucesso") || textoLower.Contains("conquista") || textoLower.Contains("vitória"))
                    resumo = "Semana marcada por conquistas e sentimentos de realização";
                else
                    resumo = "Semana positiva com bem-estar geral";
            }
            // Regras baseadas na emoção ANSIOSO
            else if (emocaoPredominanteLower.Contains("ansioso"))
            {
                if (textoLower.Contains("tarefas") || textoLower.Contains("prazos") || textoLower.Contains("pendentes") ||
                    textoLower.Contains("trabalho") || textoLower.Contains("deadline"))
                    resumo = "Período de ansiedade devido à sobrecarga de trabalho e pressão";
                else if (textoLower.Contains("decisão") || textoLower.Contains("escolher") || textoLower.Contains("preocupado") ||
                    textoLower.Contains("dúvida") || textoLower.Contains("incerto"))
                    resumo = "Semana marcada por ansiedade relacionada a tomadas de decisão";
                else if (textoLower.Contains("futuro") || textoLower.Contains("medo") || textoLower.Contains("incerteza"))
                    resumo = "Período ansioso com preocupações sobre o futuro e incertezas";
                else
                    resumo = "Período ansioso com dificuldade de relaxar e descansar";
            }
            // Regras baseadas na emoção CALMO
            else if (emocaoPredominanteLower.Contains("calmo"))
            {
                if (textoLower.Contains("relaxamento") || textoLower.Contains("paz") || textoLower.Contains("tranquilo") ||
                    textoLower.Contains("sereno") || textoLower.Contains("zen"))
                    resumo = "Semana tranquila com momentos de descanso e serenidade";
                else if (textoLower.Contains("equilíbrio") || textoLower.Contains("organizado") || textoLower.Contains("controle") ||
                    textoLower.Contains("planejado") || textoLower.Contains("estruturado"))
                    resumo = "Período calmo com boa gestão do tempo e bem-estar geral";
                else if (textoLower.Contains("meditação") || textoLower.Contains("mindfulness") || textoLower.Contains("respiração"))
                    resumo = "Semana serena com práticas de mindfulness e autoconhecimento";
                else
                    resumo = "Período de calma e estabilidade emocional";
            }
            // Regras baseadas na emoção CANSADO
            else if (emocaoPredominanteLower.Contains("cansado"))
            {
                if (textoLower.Contains("trabalho") || textoLower.Contains("exausto") || textoLower.Contains("muito") ||
                    textoLower.Contains("sobrecarga") || textoLower.Contains("sobrecarregado"))
                    resumo = "Período de cansaço devido ao excesso de atividades profissionais";
                else if (textoLower.Contains("dormir") || textoLower.Contains("energia") || textoLower.Contains("sono") ||
                    textoLower.Contains("descanso") || textoLower.Contains("repouso"))
                    resumo = "Semana marcada por fadiga relacionada à falta de descanso adequado";
                else if (textoLower.Contains("físico") || textoLower.Contains("mental") || textoLower.Contains("esgotado"))
                    resumo = "Período de esgotamento físico e mental, necessitando de pausa";
                else
                    resumo = "Período de cansaço que requer atenção ao descanso e recuperação";
            }
            // Regras baseadas na emoção ESTRESSADO
            else if (emocaoPredominanteLower.Contains("estressado"))
            {
                if (textoLower.Contains("conflito") || textoLower.Contains("tensão") || textoLower.Contains("difícil") ||
                    textoLower.Contains("problema") || textoLower.Contains("desafio"))
                    resumo = "Semana estressante com tensões profissionais e desafios interpessoais";
                else if (textoLower.Contains("pressão") || textoLower.Contains("tempo") || textoLower.Contains("sobrecarga") ||
                    textoLower.Contains("urgente") || textoLower.Contains("correria"))
                    resumo = "Período de estresse devido à sobrecarga e falta de organização";
                else if (textoLower.Contains("imprevisto") || textoLower.Contains("mudança") || textoLower.Contains("adaptação"))
                    resumo = "Semana estressante com imprevistos e necessidade de adaptação";
                else
                    resumo = "Período estressante que requer estratégias de gerenciamento de estresse";
            }
            else
            {
                resumo = "Período com variações emocionais diversas";
            }

            return resumo;
        }

        /// <summary>
        /// Gera recomendação baseada na emoção predominante
        /// </summary>
        private string GerarRecomendacao(string emocaoPredominante)
        {
            var emocaoPredominanteLower = emocaoPredominante.ToLowerInvariant();

            // Recomendações baseadas na emoção predominante
            if (emocaoPredominanteLower.Contains("feliz"))
            {
                return "Continue mantendo atividades que trazem bem-estar. Considere registrar o que funcionou bem para replicar no futuro.";
            }
            else if (emocaoPredominanteLower.Contains("ansioso"))
            {
                return "Pratique técnicas de respiração e mindfulness. Organize suas tarefas por prioridade e considere dividir objetivos grandes em etapas menores.";
            }
            else if (emocaoPredominanteLower.Contains("calmo"))
            {
                return "Mantenha os hábitos que estão trazendo tranquilidade. Continue com práticas de autocuidado e organização.";
            }
            else if (emocaoPredominanteLower.Contains("cansado"))
            {
                return "Priorize o descanso e o sono adequado. Considere revisar sua rotina para evitar sobrecarga e estabelecer limites saudáveis.";
            }
            else if (emocaoPredominanteLower.Contains("estressado"))
            {
                return "Identifique as principais fontes de estresse e desenvolva estratégias de enfrentamento. Pratique exercícios físicos e técnicas de relaxamento regularmente.";
            }
            else
            {
                return "Continue monitorando suas emoções e identifique padrões que possam ser melhorados.";
            }
        }

        /// <summary>
        /// Carrega dados para treinamento do modelo
        /// </summary>
        private Task<List<SemanaAnalise>> CarregarDadosTreino()
        {
            using var activity = ActivitySource.StartActivity("CarregarDadosTreino");
            
            try
            {
                // Caminho do arquivo JSON na pasta ML/Data
                activity?.AddEvent(new ActivityEvent("Localizando arquivo de dados de treino"));
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var jsonPath = Path.Combine(baseDirectory, "ML", "Data", "dadosTreino.json");
                activity?.SetTag("ml.dados_treino_path", jsonPath);

                if (File.Exists(jsonPath))
                {
                    activity?.AddEvent(new ActivityEvent("Lendo arquivo de dados de treino"));
                    var jsonContent = File.ReadAllText(jsonPath);
                    activity?.AddEvent(new ActivityEvent("Deserializando dados de treino"));
                    var dados = JsonSerializer.Deserialize<List<SemanaAnalise>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (dados != null && dados.Any())
                    {
                        activity?.SetTag("ml.dados_treino_count", dados.Count);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        _logger.LogInformation("Dados de treinamento carregados com sucesso: {Count} registros", dados.Count);
                        return Task.FromResult(dados);
                    }
                    else
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "Arquivo de dados de treino vazio ou inválido");
                        _logger.LogWarning("Arquivo de dados de treino está vazio ou inválido");
                    }
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Arquivo de dados de treino não encontrado");
                    _logger.LogWarning("Arquivo de dados de treino não encontrado em: {Path}", jsonPath);
                }
            }
            catch (Exception ex)
            {
                // Se houver erro ao ler o arquivo, loga e retorna lista vazia
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao carregar dados de treinamento do arquivo JSON");
            }

            // Fallback: retorna lista vazia se não conseguir carregar do arquivo
            _logger.LogWarning("Não foi possível carregar dados de treino do arquivo JSON. O modelo pode não funcionar corretamente.");
            return Task.FromResult(new List<SemanaAnalise>());
        }

        
    }
}