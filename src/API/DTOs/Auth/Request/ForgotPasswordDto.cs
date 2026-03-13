using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth.Request
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; init; } = string.Empty;
    }
}
