using System.ComponentModel.DataAnnotations;
#nullable enable

namespace API.DTOs.Auth
{
    public class LoginReqDto
    {
        [Required] public string UserName { get; init; } = string.Empty;
        [Required] public string Password { get; init; } = string.Empty;
    }
}
