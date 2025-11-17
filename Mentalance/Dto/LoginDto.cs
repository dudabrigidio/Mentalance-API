using System.ComponentModel.DataAnnotations;

namespace Mentalance.Dto
{
    /// <summary>
    /// DTO (Data Transfer Object) para autenticação de usuários
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Endereço de e-mail do usuário
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Senha do usuário
        /// </summary>
        [Required]
        public string Senha { get; set; }
    }
}
