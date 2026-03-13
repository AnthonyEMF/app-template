using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth.Request
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "El Token es requerido")]
        public string Token { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "El RefreshToken es requerido")]
        public string RefreshToken { get; init; } = string.Empty;
    }
}
