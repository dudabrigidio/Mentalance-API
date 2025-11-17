using Microsoft.ML.Data;

namespace Mentalance.ML.Data
{
    /// <summary>
    /// Classe que representa a predição do modelo ML.NET
    /// </summary>
    public class AnaliseML
    {
        /// <summary>
        /// Resumo textual gerado pelo modelo ML.NET
        /// </summary>
        [ColumnName("PredictedLabel")]
        public string Resumo { get; set; }

        /// <summary>
        /// Recomendação baseada na análise dos checkins
        /// </summary>
        public string Recomendacao { get; set; }

    }
}
