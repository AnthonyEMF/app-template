using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth.Request
{
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "El código es requerido")]
        [Length(6, 6, ErrorMessage = "El código debe tener exactamente 6 dígitos")]
        public string Code { get; set; } = string.Empty;
    }
}
