namespace Mentalance
{
    /// <summary>
    /// Representa uma previsão do tempo
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// Data da previsão
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Temperatura em graus Celsius
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Temperatura em graus Fahrenheit
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// Resumo da previsão do tempo
        /// </summary>
        public string? Summary { get; set; }
    }
}
