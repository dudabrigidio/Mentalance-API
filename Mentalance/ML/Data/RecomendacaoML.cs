using Microsoft.ML.Data;

namespace Mentalance.ML.Data
{
    /// <summary>
    /// Classe que representa a predição de recomendação do modelo ML.NET
    /// </summary>
    public class RecomendacaoML
    {
        /// <summary>
        /// Recomendação textual gerada pelo modelo ML.NET
        /// </summary>
        [ColumnName("PredictedLabel")]
        public string Recomendacao { get; set; }
    }
}



