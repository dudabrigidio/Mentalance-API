using System.ComponentModel.DataAnnotations;

namespace Mentalance.Dto
{
    /// <summary>
    /// DTO (Data Transfer Object) para criação e atualização de usuários
    /// </summary>
    public class UsuarioDto
    {
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
        /// Senha do usuário
        /// </summary>
        [Required]
        public string Senha { get; set; }

        /// <summary>
        /// Cargo/função do usuário no sistema
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Cargo { get; set; }
    }
}
