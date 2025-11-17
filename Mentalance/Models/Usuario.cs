using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mentalance.Models
{
    /// <summary>
    /// Representa um usuário do sistema Mentalance
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        /// <summary>
        /// Endereço de e-mail do usuário (usado para login)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Senha do usuário (armazenada em texto plano - considerar hash em produção)
        /// </summary>
        [Required]
        public string Senha { get; set; }

        /// <summary>
        /// Cargo/função do usuário no sistema
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Cargo { get; set; }

        /// <summary>
        /// Data e hora de cadastro do usuário no sistema
        /// </summary>
        [Required]
        public DateTime DataCadastro { get; set; }


    }
}
