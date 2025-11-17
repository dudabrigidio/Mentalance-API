using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mentalance.Models
{
    /// <summary>
    /// Representa uma análise semanal gerada automaticamente baseada nos check-ins do usuário
    /// </summary>
    public class AnaliseSemanal
    {
        /// <summary>
        /// Identificador único da análise semanal
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAnalise { get; set; }

        /// <summary>
        /// Identificador do usuário para o qual a análise foi gerada
        /// </summary>
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Período de referência da análise (ex: "Semana 01/01/2024 a 07/01/2024")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SemanaReferencia { get; set; }

        /// <summary>
        /// Emoção que mais apareceu nos check-ins da semana
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string EmocaoPredominante { get; set; }

        /// <summary>
        /// Resumo textual gerado por ML sobre o estado emocional da semana
        /// </summary>
        [Required]   
        public string Resumo { get; set; }

        /// <summary>
        /// Recomendação personalizada gerada por ML baseada na análise da semana
        /// </summary>
        [Required]
        public string Recomendacao { get; set; }

    }
}
