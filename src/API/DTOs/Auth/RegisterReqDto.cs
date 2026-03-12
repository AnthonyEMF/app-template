using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth
{
    public class RegisterReqDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string FirstName { get; init; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        public string LastName { get; init; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(30, MinimumLength = 3,
            ErrorMessage = "El nombre de usuario debe tener entre 3 y 30 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._]+$",
            ErrorMessage = "El nombre de usuario no puede contener espacios, ni caracteres especiales")]
        public string UserName { get; init; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        public string Role { get; init; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe contener al menos 8 caracteres e incluir minúsculas, mayúsculas, números y caracteres especiales")]
        public string Password { get; init; } = string.Empty;

        [Required(ErrorMessage = "Confirmar la contraseña es requerido")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
