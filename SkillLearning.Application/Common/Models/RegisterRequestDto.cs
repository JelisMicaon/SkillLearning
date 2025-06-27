using System.ComponentModel.DataAnnotations;

namespace SkillLearning.Application.Common.Models
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Username é obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username deve ter entre 3 e 50 caracteres.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password é obrigatório.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; }
    }
}