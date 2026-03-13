using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth.Request
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "El ResetToken es requerido")]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe contener al menos 8 caracteres e incluir minúsculas, mayúsculas, números y caracteres especiales")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmar la contraseña es requerido")]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
