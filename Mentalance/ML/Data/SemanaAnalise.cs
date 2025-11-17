using Microsoft.ML.Data;

namespace Mentalance.ML.Data
{
    /// <summary>
    /// Classe de entrada para o modelo ML.NET representando uma semana agregada
    /// Contém múltiplos checkins agregados (emoções e textos combinados)
    /// </summary>
    public class SemanaAnalise
    {
        /// <summary>
        /// Todas as emoções da semana combinadas (ex: "Feliz,Ansioso,Calmo")
        /// </summary>
        [LoadColumn(0)]
        public string Emocoes { get; set; }

        /// <summary>
        /// Todos os textos da semana combinados (ex: "Dia produtivo. Muitas tarefas. Dia tranquilo.")
        /// </summary>
        [LoadColumn(1)]
        public string Textos { get; set; }

        /// <summary>
        /// Emoção predominante calculada estatisticamente
        /// </summary>
        [LoadColumn(2)]
        public string EmocaoPredominante { get; set; }

        /// <summary>
        /// Label para treinamento: resumo textual esperado baseado em regras
        /// </summary>
        [LoadColumn(3)]
        public string ResumoEsperado { get; set; }

        /// <summary>
        /// Label para treinamento: recomendação esperada baseada em regras
        /// </summary>
        [LoadColumn(4)]
        public string RecomendacaoEsperada { get; set; }
    }
}

