using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth
{
    public class RefreshTokenReqDto
    {
        [Required] public string Token { get; init; } = string.Empty;
        [Required] public string RefreshToken { get; init; } = string.Empty;
    }
}
