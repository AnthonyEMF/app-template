using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth.Request
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; init; } = string.Empty;
    }
}
