using Mentalance.API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mentalance.Models
{
    /// <summary>
    /// Representa um check-in emocional realizado por um usuário
    /// </summary>
    public class Checkin
    {
        /// <summary>
        /// Identificador único do check-in
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCheckin { get; set; }

        /// <summary>
        /// Identificador do usuário que realizou o check-in
        /// </summary>
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Data e hora em que o check-in foi realizado
        /// </summary>
        [Required]
        public DateTime DataCheckin { get; set; }

        /// <summary>
        /// Emoção registrada pelo usuário no check-in
        /// </summary>
        [Required]
        [MaxLength(100)]
        public EmocaoEnum Emocao { get; set; }

        /// <summary>
        /// Texto descritivo do check-in fornecido pelo usuário
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Texto { get; set; }

        /// <summary>
        /// Análise de sentimento gerada automaticamente (positivo, neutro, negativo)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string AnáliseSentimento { get; set; }

        /// <summary>
        /// Resposta personalizada gerada automaticamente baseada na emoção e texto
        /// </summary>
        [Required]
        public string RespostaGerada { get; set; }

    }
}
