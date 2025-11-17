using Mentalance.API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mentalance.Converters
{
    /// <summary>
    /// Conversor customizado para EmocaoEnum que aceita variações de gênero
    /// (ex: "Cansada" -> Cansado, "Cansado" -> Cansado)
    /// </summary>
    public class EmocaoEnumJsonConverter : JsonConverter<EmocaoEnum>
    {
        /// <summary>
        /// Lê e converte uma string JSON para EmocaoEnum, aceitando variações de gênero
        /// </summary>
        /// <param name="reader">Leitor JSON</param>
        /// <param name="typeToConvert">Tipo a ser convertido</param>
        /// <param name="options">Opções do serializador</param>
        /// <returns>Valor do enum EmocaoEnum correspondente</returns>
        public override EmocaoEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }

            string? value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException("Emoção não pode ser vazia");
            }

            // Normaliza a string (remove acentos e converte para maiúsculas para comparação)
            string normalized = value.Trim();

            // Mapeia variações de gênero e outras variações comuns
            switch (normalized.ToLowerInvariant())
            {
                case "cansada":
                case "cansado":
                    return EmocaoEnum.Cansado;
                case "feliz":
                    return EmocaoEnum.Feliz;
                case "ansioso":
                case "ansiosa":
                    return EmocaoEnum.Ansioso;
                case "calmo":
                case "calma":
                    return EmocaoEnum.Calmo;
                case "estressado":
                case "estressada":
                    return EmocaoEnum.Estressado;
                default:
                    // Tenta fazer o parse padrão (case-insensitive)
                    if (Enum.TryParse<EmocaoEnum>(normalized, ignoreCase: true, out var result))
                    {
                        return result;
                    }
                    throw new JsonException($"Valor de emoção inválido: '{value}'. Valores aceitos: Feliz, Cansado/Cansada, Ansioso/Ansiosa, Calmo/Calma, Estressado/Estressada");
            }
        }

        /// <summary>
        /// Escreve um valor EmocaoEnum como string JSON
        /// </summary>
        /// <param name="writer">Escritor JSON</param>
        /// <param name="value">Valor do enum a ser escrito</param>
        /// <param name="options">Opções do serializador</param>
        public override void Write(Utf8JsonWriter writer, EmocaoEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

