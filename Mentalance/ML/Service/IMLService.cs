using Mentalance.ML.Data;
using Mentalance.Models;

namespace Mentalance.ML.Service
{
    public interface IMLService
    {
        /// <summary>
        /// Gera análise/resumo textual com base nos checkins de uma semana (7 dias)
        /// Agrega múltiplas emoções e textos para gerar um resumo do que foi predominante e como melhorar
        /// </summary>
        /// <param name="usuarioId" >ID do usuário para buscar checkins dos últimos 7 dias</param>
        /// <param name="checkinsList">Checkins para analisar</param>
        /// <returns>Predição com resumo textual gerado</returns>
        Task<AnaliseML> GerarResumoAsync(int usuarioId, IEnumerable<Checkin> checkinsList);
    }
}
