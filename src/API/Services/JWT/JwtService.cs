using API.Database;
using API.Database.Models;
using API.DTOs.Auth.Response;
using API.DTOs.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services.Auth;

public class JwtService(
    UserManager<UserEntity> _userManager, 
    AppDbContext _context, 
    IConfiguration _config, 
    IHttpContextAccessor _httpContextAccessor
    ) : IJwtService
{
    public const string HttpContextUserKey = "AuditUser";

    // Construir los claims del usuario incluyendo sus roles
    private async Task<List<Claim>> GetClaimsAsync(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email,               user.Email!),
            new(JwtRegisteredClaimNames.Jti,    Guid.NewGuid().ToString()),
            new("userId",                       user.Id),
            new("userName",                     user.UserName),
            new("fullName",                     $"{user.FirstName} {user.LastName}")
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    // Generar Token principal
    private JwtSecurityToken GenerateAccessToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:Secret"]!));

        return new JwtSecurityToken(
            issuer: _config["JWT:Issuer"],
            audience: _config["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:Expiration"] ?? "15")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
    }

    // Generar RefreshToken 
    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    // Extraer el ClaimsPrincipal de un token expirado (sin validar lifetime)
    public ClaimsPrincipal GetClaimsFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:Secret"]!));

        return new JwtSecurityTokenHandler().ValidateToken(token,
            new TokenValidationParameters
            {
                IssuerSigningKey = key,
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out _);
    }

    // Generar token, persistir el refresh token y construir el DTO de respuesta
    public async Task<BaseDto<AuthDto>> BuildAuthResponseAsync(UserEntity user, string message)
    {
        var claims = await GetClaimsAsync(user);
        var jwtToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:RefreshExpiration"] ?? "30"));

        await _context.SaveChangesAsync();

        // Exponer datos del usuario para que el middleware los pueda leer
        // Cubre el caso de login/register donde no hay JWT en la request
        _httpContextAccessor.HttpContext?.Items.TryAdd(HttpContextUserKey, new LogUser
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
        });

        return new BaseDto<AuthDto>
        {
            StatusCode = 200,
            Status = true,
            Message = message,
            Data = new AuthDto
            {
                FullName = $"{user.FirstName} {user.LastName}",
                UserName = user.UserName,
                Email = user.Email,
                Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                TokenExpiration = jwtToken.ValidTo,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = user.RefreshTokenExpiration,
            }
        };
    }

    // Obtener ID del usuario en sesión
    public string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.Items[HttpContextUserKey] as LogUser;
        return user?.Id;
    }
}
