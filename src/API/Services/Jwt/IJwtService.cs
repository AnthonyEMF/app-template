using API.Database.Entities;
using API.DTOs.Auth.Response;
using API.DTOs.Shared;
using System.Security.Claims;

namespace API.Services.Auth;

public interface IJwtService
{
    Task<BaseDto<AuthDto>> BuildAuthResponseAsync(UserEntity user, string message);

    ClaimsPrincipal GetClaimsFromExpiredToken(string token);
}
