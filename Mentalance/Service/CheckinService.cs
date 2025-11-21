using Mentalance.API.Models;
using Mentalance.Models;
using Mentalance.Repository;
using Mentalance.Service;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace Mentalance.Service    
{
    /// <summary>
    /// Implementação do serviço para a entidade Checkin
    /// Contém a lógica de negócio e validações
    /// </summary>
    public class CheckinService : ICheckinService
    {
        private static readonly ActivitySource ActivitySource = new("Mentalance.CheckinService");
        private readonly ICheckinRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<CheckinService> _logger;

        /// <summary>
        /// Inicializa uma nova instância do CheckinService
        /// </summary>
        /// <param name="repository">Repositório de check-ins</param>
        /// <param name="usuarioRepository">Repositório de usuários</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public CheckinService(ICheckinRepository repository, IUsuarioRepository usuarioRepository, ILogger<CheckinService> logger)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retorna todos os checkins de um usuário específico
        /// </summary>
        /// <param name="idUsuario">ID do usuário</param>
        public async Task<IEnumerable<Checkin>> GetAllAsync(int idUsuario)
        {
            using var activity = ActivitySource.StartActivity("GetAllCheckinsByUsuario");
            activity?.SetTag("checkin.usuario_id", idUsuario);
            
            _logger.LogInformation("Iniciando busca de checkins do usuário: {UsuarioId}", idUsuario);
            
            if (idUsuario <= 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "ID do usuário inválido");
                _logger.LogWarning("Tentativa de buscar checkins com ID do usuário inválido: {UsuarioId}", idUsuario);
                throw new ArgumentException("ID do usuário deve ser maior que zero", nameof(idUsuario));
            }
            
            try {
                activity?.AddEvent(new ActivityEvent("Buscando checkins no repositório"));
                var checkins = await _repository.GetByUsuarioAsync(idUsuario);
                
                var count = checkins.Count();
                activity?.SetTag("checkin.count", count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger.LogInformation("Checkins encontrados para o usuário {UsuarioId}: {CheckinsCount}", idUsuario, count);
                return checkins;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao buscar checkins do usuário: {UsuarioId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Busca um checkin pelo ID
        /// </summary>
        public async Task<Checkin?> GetByIdAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("GetCheckinById");
            activity?.SetTag("checkin.id", id);
            
            _logger.LogInformation("Iniciando busca de checkin com ID: {CheckinId}", id);
            
            if (id <= 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "ID inválido");
                _logger.LogWarning("Tentativa de buscar checkin com ID inválido: {CheckinId}", id);
                return null;
            }

            try {
                activity?.AddEvent(new ActivityEvent("Buscando checkin no repositório"));
                var checkin = await _repository.GetByIdAsync(id);
                
                if (checkin != null)
                {
                    activity?.SetTag("checkin.usuario_id", checkin.IdUsuario);
                    activity?.SetTag("checkin.emocao", checkin.Emocao.ToString());
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Checkin não encontrado");
                }
                
                _logger.LogInformation("Checkin {CheckinId} encontrado", id);
                return checkin;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao buscar checkin com ID: {CheckinId}", id);
                throw;
            }
        }

        /// <summary>
        /// Cria um novo checkin após validar os dados
        /// </summary>
        public async Task<Checkin> CreateAsync(CheckinDto checkinDto)
        {
            using var activity = ActivitySource.StartActivity("CreateCheckin");

            // Log de início da operação
            _logger.LogInformation(
                "Iniciando criação de checkin para usuário {UsuarioId}", 
                checkinDto?.IdUsuario);

            try {
                // Validação: verifica se o checkin não é nulo
                activity?.AddEvent(new ActivityEvent("Validando dados do checkin"));
                if (checkinDto == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "CheckinDto é nulo");
                    _logger.LogWarning("Tentativa de criar checkin com DTO nulo");
                    throw new ArgumentNullException(nameof(checkinDto), "Checkin não pode ser nulo");
                }

                // Adicionar tags ao trace
                activity?.SetTag("checkin.usuario_id", checkinDto.IdUsuario);
                activity?.SetTag("checkin.emocao", checkinDto.Emocao.ToString());

                // Validação: verifica campos obrigatórios

                if (checkinDto.IdUsuario <= 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "ID do usuário inválido");
                    _logger.LogWarning("Tentativa de criar checkin com ID do usuário inválido: {UsuarioId}", checkinDto.IdUsuario);
                    throw new ArgumentException("ID do usuário é obrigatório e deve ser maior que zero", nameof(checkinDto.IdUsuario));
                }

                if (string.IsNullOrWhiteSpace(checkinDto.Emocao.ToString()))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Emoção nula");
                    _logger.LogWarning("Tentativa de criar checkin com Emoção nula");
                    throw new ArgumentException("Emoção é obrigatória", nameof(checkinDto.Emocao));
                }

                if (checkinDto.Emocao != EmocaoEnum.Feliz && checkinDto.Emocao != EmocaoEnum.Cansado
                                                        && checkinDto.Emocao != EmocaoEnum.Ansioso 
                                                        && checkinDto.Emocao != EmocaoEnum.Calmo 
                                                        && checkinDto.Emocao != EmocaoEnum.Estressado)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Emoção inválida");
                    _logger.LogWarning("Tentativa de criar checkin com Emoção inválida: {Emocao}", checkinDto.Emocao);
                    throw new ArgumentException("Insira uma emoção válida: Feliz, Cansado, Ansioso, Calmo, Estressado", nameof(checkinDto.Emocao));
                }

            
                if (string.IsNullOrWhiteSpace(checkinDto.Texto))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Texto nulo");
                    _logger.LogWarning("Tentativa de criar checkin com Texto nulo");
                    throw new ArgumentException("Texto é obrigatório", nameof(checkinDto.Texto));
                }

                // Validação: verifica se o usuário existe
                activity?.AddEvent(new ActivityEvent("Verificando existência do usuário"));
                var usuarioExiste = await _usuarioRepository.ExisteAsync(checkinDto.IdUsuario);
                if (!usuarioExiste)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Usuário não encontrado");
                    _logger.LogWarning("Tentativa de criar checkin com usuário inexistente: {UsuarioId}", checkinDto.IdUsuario);
                    throw new ArgumentException($"Usuário com ID {checkinDto.IdUsuario} não encontrado", nameof(checkinDto.IdUsuario));
                }

                activity?.AddEvent(new ActivityEvent("Criando entidade checkin"));
                var checkin = new Checkin();
                checkin.IdUsuario = checkinDto.IdUsuario;
                checkin.Emocao = checkinDto.Emocao;
                checkin.Texto = checkinDto.Texto;
                checkin.AnáliseSentimento = ClassificarSentimento(checkinDto.Emocao);
                checkin.RespostaGerada = GerarResposta(checkinDto.Emocao, checkin.AnáliseSentimento, checkin.Texto);
                checkin.DataCheckin = DateTime.Now;

                // Adiciona o checkin ao banco
                activity?.AddEvent(new ActivityEvent("Persistindo checkin no banco"));
                var resultado = await _repository.AddAsync(checkin);

                // Marcar trace como sucesso
                activity?.SetTag("checkin.id", resultado.IdCheckin);
                activity?.SetTag("checkin.analise_sentimento", resultado.AnáliseSentimento);
                activity?.SetStatus(ActivityStatusCode.Ok);

                // Log de sucesso
                _logger.LogInformation("Checkin {CheckinId} criado com sucesso para usuário {UsuarioId}", resultado.IdCheckin, checkinDto.IdUsuario);

                return resultado;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao criar checkin para usuário {UsuarioId}", checkinDto?.IdUsuario);
                throw;
            }
        }

        /// <summary>
        /// Atualiza um checkin existente
        /// </summary>
        public async Task UpdateAsync(int id, CheckinDto checkinDto)
        {
            using var activity = ActivitySource.StartActivity("UpdateCheckin");
            activity?.SetTag("checkin.id", id);

            // Log de início da operação
            _logger.LogInformation("Iniciando atualização de checkin com ID: {CheckinId}", id);

            try {
                activity?.AddEvent(new ActivityEvent("Buscando checkin existente"));
                var existingCheckin = await _repository.GetByIdAsync(id);
                if (existingCheckin == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Checkin não encontrado");
                    _logger.LogWarning("Tentativa de atualizar checkin com ID inexistente: {CheckinId}", id);
                    throw new KeyNotFoundException($"Checkin com ID {id} não encontrado");
                }

            // Validação: verifica se o checkin não é nulo
                activity?.AddEvent(new ActivityEvent("Validando dados do checkin"));
                if (checkinDto == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "CheckinDto é nulo");
                    _logger.LogWarning("Tentativa de atualizar checkin com DTO nulo");
                    throw new ArgumentNullException(nameof(checkinDto), "Checkin não pode ser nulo");
                }

                // Adicionar tags ao trace
                activity?.SetTag("checkin.usuario_id", checkinDto.IdUsuario);
                activity?.SetTag("checkin.emocao", checkinDto.Emocao.ToString());

                // Validação: verifica campos obrigatórios

                if (checkinDto.IdUsuario <= 0) 
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "ID do usuário inválido");
                    _logger.LogWarning("Tentativa de atualizar checkin com ID do usuário inválido: {UsuarioId}", checkinDto.IdUsuario);
                    throw new ArgumentException("ID do usuário é obrigatório e deve ser maior que zero", nameof(checkinDto.IdUsuario));
                }

                if (string.IsNullOrWhiteSpace(checkinDto.Emocao.ToString()))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Emoção nula");
                    _logger.LogWarning("Tentativa de atualizar checkin com Emoção nula");
                    throw new ArgumentException("Emoção é obrigatória", nameof(checkinDto.Emocao));
                }

                if (checkinDto.Emocao != EmocaoEnum.Feliz && checkinDto.Emocao != EmocaoEnum.Cansado
                                                        && checkinDto.Emocao != EmocaoEnum.Ansioso
                                                        && checkinDto.Emocao != EmocaoEnum.Calmo
                                                        && checkinDto.Emocao != EmocaoEnum.Estressado)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Emoção inválida");
                    _logger.LogWarning("Tentativa de atualizar checkin com Emoção inválida: {Emocao}", checkinDto.Emocao);
                    throw new ArgumentException("Insira uma emoção válida: Feliz, Cansado, Ansioso, Calmo, Estressado", nameof(checkinDto.Emocao));
                }

                if (string.IsNullOrWhiteSpace(checkinDto.Texto))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Texto nulo");
                    _logger.LogWarning("Tentativa de atualizar checkin com Texto nulo");
                    throw new ArgumentException("Texto é obrigatório", nameof(checkinDto.Texto));
                }

                // Validação: verifica se o usuário existe
                activity?.AddEvent(new ActivityEvent("Verificando existência do usuário"));
                var usuarioExiste = await _usuarioRepository.ExisteAsync(checkinDto.IdUsuario);
                if (!usuarioExiste) 
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Usuário não encontrado");
                    _logger.LogWarning("Tentativa de atualizar checkin com usuário inexistente: {UsuarioId}", checkinDto.IdUsuario);
                    throw new ArgumentException($"Usuário com ID {checkinDto.IdUsuario} não encontrado", nameof(checkinDto.IdUsuario));
                }

                // Atualiza o checkin existente com os novos dados
                activity?.AddEvent(new ActivityEvent("Atualizando entidade checkin"));
                existingCheckin.IdUsuario = checkinDto.IdUsuario;
                existingCheckin.Emocao = checkinDto.Emocao;
                existingCheckin.Texto = checkinDto.Texto;
                existingCheckin.AnáliseSentimento = ClassificarSentimento(checkinDto.Emocao);
                existingCheckin.RespostaGerada = GerarResposta(checkinDto.Emocao, existingCheckin.AnáliseSentimento, checkinDto.Texto);
                // DataCheckin não é atualizada - mantém a data original do checkin

                // Salva as alterações
                activity?.AddEvent(new ActivityEvent("Persistindo atualização no banco"));
                await _repository.UpdateAsync(existingCheckin);

                activity?.SetTag("checkin.analise_sentimento", existingCheckin.AnáliseSentimento);
                activity?.SetStatus(ActivityStatusCode.Ok);

                // Log de sucesso
                _logger.LogInformation("Checkin {CheckinId} atualizado com sucesso", id);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao atualizar checkin com ID: {CheckinId}", id);
                throw;
            }
        }

        /// <summary>
        /// Remove um checkin do sistema
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("DeleteCheckin");
            activity?.SetTag("checkin.id", id);

            // Log de início da operação
            _logger.LogInformation("Iniciando exclusão de checkin com ID: {CheckinId}", id);

            try {
                // Validação: verifica se o ID é válido
                if (id <= 0)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "ID inválido");
                        _logger.LogWarning("Tentativa de excluir checkin com ID inválido: {CheckinId}", id);
                        throw new ArgumentException("ID inválido", nameof(id));
                    }

                // Busca o checkin
                activity?.AddEvent(new ActivityEvent("Buscando checkin para exclusão"));
                var checkin = await _repository.GetByIdAsync(id);
                if (checkin == null)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "Checkin não encontrado");
                        _logger.LogWarning("Tentativa de excluir checkin com ID inexistente: {CheckinId}", id);
                        throw new KeyNotFoundException($"Checkin com ID {id} não encontrado");
                    }

                // Remove o checkin
                activity?.AddEvent(new ActivityEvent("Excluindo checkin do banco"));
                await _repository.DeleteAsync(id);

                activity?.SetStatus(ActivityStatusCode.Ok);

                // Log de sucesso
                _logger.LogInformation("Checkin {CheckinId} excluído com sucesso", id);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Erro ao excluir checkin com ID: {CheckinId}", id);
                throw;
            }
        }


        /// <summary>
        /// Classifica o sentimento baseado na emoção informada
        /// </summary>
        private string ClassificarSentimento(EmocaoEnum emocao)
        {
            switch (emocao)
            {
                case EmocaoEnum.Feliz:
                    return "positivo";
                case EmocaoEnum.Calmo:
                    return "neutro";
                case EmocaoEnum.Ansioso:
                case EmocaoEnum.Estressado:
                case EmocaoEnum.Cansado:
                    return "negativo";
                default:
                    return "neutro";
            }
        }

        /// <summary>
        /// Gera uma resposta personalizada baseada na emoção, sentimento e texto do usuário
        /// </summary>
        private string GerarResposta(EmocaoEnum emocao, string analiseSentimento, string texto)
        {
            
            var textoLower = texto?.ToLower() ?? "";
            var random = new Random();
            
            // Analisa palavras-chave no texto para personalizar a resposta
            var temPalavrasPositivas = ContemPalavras(textoLower, new[] { "bom", "ótimo", "bem", "feliz", "alegre", "gratidão",  "felicidade", "satisfeito", "satisfeita"});
            var temPalavrasNegativas = ContemPalavras(textoLower, new[] { "ruim", "mal", "difícil", "problema", "preocupado", "triste", "cansado" });
            var temPalavrasAnsiedade = ContemPalavras(textoLower, new[] { "ansioso", "nervoso", "preocupado", "medo", "tensão", "inquieto" });
            var temPalavrasEstresse = ContemPalavras(textoLower, new[] { "estressado", "pressão", "sobrecarregado", "exausto", "opressão" });
            var temPalavrasCansaco = ContemPalavras(textoLower, new[] { "cansado", "fadiga", "exausto", "sem energia", "esgotado" });

            // Gera resposta baseada na emoção e contexto (limitado a 100 caracteres)
            switch (emocao)
            {
                case EmocaoEnum.Feliz:
                    if (temPalavrasPositivas)
                    {
                        var mensagensPositivas = new[]
                        {
                            "Que maravilha! Continue cultivando esses momentos positivos e celebrando suas conquistas.",
                            "É incrível ver sua felicidade! Aproveite cada momento e compartilhe essa positividade.",
                            "Que alegria! Momentos assim são preciosos - guarde essa sensação e celebre suas vitórias.",
                            "Fantástico! Sua felicidade é contagiante. Continue valorizando esses momentos especiais."
                        };
                        return mensagensPositivas[random.Next(mensagensPositivas.Length)];
                    }
                    var mensagensFeliz = new[]
                    {
                        "É ótimo ver que você está feliz! Aproveite esse momento.",
                        "Que bom saber que você está se sentindo bem! Aproveite essa sensação positiva.",
                        "Fico feliz em saber que você está bem! Continue cultivando essa energia positiva.",
                        "É maravilhoso ver sua felicidade! Aproveite cada instante desse sentimento."
                    };
                    return mensagensFeliz[random.Next(mensagensFeliz.Length)];

                case EmocaoEnum.Calmo:
                    if (temPalavrasPositivas)
                    {
                        var mensagensCalmoPositivo = new[]
                        {
                            "Que bom que você está em paz! A calma é um estado precioso - aproveite para recarregar.",
                            "É reconfortante saber que você está tranquilo. A serenidade é um presente - aproveite.",
                            "Que maravilha sentir essa paz! Aproveite esse momento de calma para renovar suas energias.",
                            "É ótimo ver você em paz! A calma é essencial - aproveite para se conectar consigo mesmo."
                        };
                        return mensagensCalmoPositivo[random.Next(mensagensCalmoPositivo.Length)];
                    }
                    var mensagensCalmo = new[]
                    {
                        "É reconfortante saber que você está calmo. A serenidade é importante para o bem-estar.",
                        "Que bom que você está tranquilo. A calma é um aliado importante para sua saúde mental.",
                        "É ótimo ver que você está em paz. A serenidade ajuda muito no seu bem-estar diário.",
                        "Fico feliz em saber que você está calmo. A tranquilidade é fundamental para você."
                    };
                    return mensagensCalmo[random.Next(mensagensCalmo.Length)];

                case EmocaoEnum.Ansioso:
                    if (temPalavrasAnsiedade)
                    {
                        var mensagensAnsiedadeDetalhada = new[]
                        {
                            "Entendo que a ansiedade pode ser desafiadora. Tente respirações profundas e focar no presente.",
                            "A ansiedade pode ser intensa. Respire fundo, conte até dez e tente focar no aqui e agora.",
                            "Entendo sua ansiedade. Pratique respiração profunda e lembre-se: você está seguro no momento presente.",
                            "A ansiedade é difícil, mas passageira. Tente técnicas de respiração e foque no que você pode controlar."
                        };
                        return mensagensAnsiedadeDetalhada[random.Next(mensagensAnsiedadeDetalhada.Length)];
                    }
                    var mensagensAnsiedade = new[]
                    {
                        "A ansiedade pode ser difícil. Que tal técnicas de respiração ou uma caminhada? É normal sentir-se assim.",
                        "Entendo que a ansiedade é desafiadora. Tente fazer uma pausa e praticar algo que te acalme.",
                        "A ansiedade pode ser intensa. Respire fundo, faça uma caminhada ou ouça uma música relaxante.",
                        "É normal sentir ansiedade. Tente técnicas de respiração profunda ou uma atividade que te distraia."
                    };
                    return mensagensAnsiedade[random.Next(mensagensAnsiedade.Length)];

                case EmocaoEnum.Estressado:
                    if (temPalavrasEstresse)
                    {
                        var mensagensEstresseDetalhada = new[]
                        {
                            "O estresse pode ser esgotante. Faça uma pausa, pratique algo relaxante ou converse com alguém.",
                            "O estresse é desgastante. Pare um momento, respire fundo e faça algo que te traga tranquilidade.",
                            "Entendo que o estresse está pesado. Dê-se uma pausa, pratique algo que relaxe ou busque apoio.",
                            "O estresse pode ser opressor. Tente fazer uma pausa, respirar fundo e fazer algo que te acalme."
                        };
                        return mensagensEstresseDetalhada[random.Next(mensagensEstresseDetalhada.Length)];
                    }
                    var mensagensEstresse = new[]
                    {
                        "Entendo que está estressado. Tente identificar a causa e dê pequenos passos para aliviar a pressão.",
                        "O estresse pode ser difícil. Faça uma pausa, identifique o que está causando e cuide-se.",
                        "Entendo seu estresse. Tente fazer uma pausa, respirar fundo e dar pequenos passos para se sentir melhor.",
                        "O estresse é desafiador. Identifique a causa, faça uma pausa e pratique algo que te relaxe."
                    };
                    return mensagensEstresse[random.Next(mensagensEstresse.Length)];

                case EmocaoEnum.Cansado:
                    if (temPalavrasCansaco)
                    {
                        var mensagensCansacoDetalhada = new[]
                        {
                            "O cansaço pode ser um sinal de que precisa descansar. Priorize seu bem-estar e permita-se pausar.",
                            "O cansaço é um aviso do seu corpo. Dê-se permissão para descansar e recarregar suas energias.",
                            "Entendo que está cansado. O descanso é essencial - permita-se ter momentos de pausa e recuperação.",
                            "O cansaço merece atenção. Priorize seu descanso e não se cobre tanto - você precisa recarregar."
                        };
                        return mensagensCansacoDetalhada[random.Next(mensagensCansacoDetalhada.Length)];
                    }
                    var mensagensCansaco = new[]
                    {
                        "É importante respeitar quando está cansado. Tente descansar adequadamente - o descanso é essencial.",
                        "O cansaço é válido. Priorize seu descanso e não se cobre - você merece recarregar suas energias.",
                        "É normal sentir cansaço. Dê-se permissão para descansar e cuidar do seu bem-estar físico e mental.",
                        "O cansaço precisa ser respeitado. Tente descansar adequadamente e não se pressione tanto."
                    };
                    return mensagensCansaco[random.Next(mensagensCansaco.Length)];

                default:
                    var mensagensDefault = new[]
                    {
                        "Obrigado por compartilhar. Lembre-se de cuidar do seu bem-estar emocional.",
                        "Obrigado por se abrir. É importante cuidar da sua saúde mental e emocional.",
                        "Agradeço por compartilhar. Lembre-se de priorizar seu bem-estar e cuidar de si mesmo.",
                        "Obrigado por confiar. Cuide do seu bem-estar emocional - você merece estar bem."
                    };
                    return mensagensDefault[random.Next(mensagensDefault.Length)];
            }
        }

        /// <summary>
        /// Verifica se o texto contém alguma das palavras-chave fornecidas
        /// </summary>
        private bool ContemPalavras(string texto, string[] palavras)
        {
            if (string.IsNullOrWhiteSpace(texto) || palavras == null || palavras.Length == 0)
                return false;

            foreach (var palavra in palavras)
            {
                if (texto.Contains(palavra, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

}