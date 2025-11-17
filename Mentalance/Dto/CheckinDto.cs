using Mentalance.API.Models;
using Mentalance.Converters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mentalance
{
    /// <summary>
    /// DTO (Data Transfer Object) para criação e atualização de check-ins
    /// </summary>
    public class CheckinDto
    {
        /// <summary>
        /// Identificador do usuário que está realizando o check-in
        /// </summary>
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Emoção registrada pelo usuário no check-in
        /// </summary>
        [Required]
        [JsonConverter(typeof(EmocaoEnumJsonConverter))]
        public EmocaoEnum Emoção { get; set; }

        /// <summary>
        /// Texto descritivo do check-in fornecido pelo usuário
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Texto { get; set; }

    }
}
