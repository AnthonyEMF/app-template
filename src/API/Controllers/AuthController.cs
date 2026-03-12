using API.Constants;
using API.Database;
using API.Database.Entities;
using API.DTOs.Auth;
using API.DTOs.Shared;
using API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/auth")]
[ApiController]
[AllowAnonymous]
public class AuthController(
    SignInManager<UserEntity> _signInManager,
    UserManager<UserEntity> _userManager,
    RoleManager<IdentityRole> _roleManager,
    AppDbContext _context,
    IJwtService _jwtService,
    ILogger<AuthController> _logger
    ) : BaseController
{
    // Iniciar sesión
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<AuthResDto>>> Login(LoginReqDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
        if (user is null)
            return Fail(404, MessagesConstant.USER_NOT_FOUND);

        var result = await _signInManager.PasswordSignInAsync(
            dto.UserName, dto.Password,
            isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Fail(401, MessagesConstant.USER_BLOCKED);

        if (!result.Succeeded)
            return Fail(401, MessagesConstant.WRONG_PASSWORD);

        return Ok(await _jwtService.BuildAuthResponseAsync(user, MessagesConstant.LOGIN_SUCCESS));
    }

    // Registrar nuevo usuario
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<AuthResDto>>> Register(RegisterReqDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
            return Fail(400, MessagesConstant.INVALID_USERNAME);

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Fail(400, MessagesConstant.INVALID_EMAIL);

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return Fail(400, MessagesConstant.INVALID_ROLE);

        var user = new UserEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            CreatedDate = DateTime.UtcNow,
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Fail(400, $"{MessagesConstant.REGISTRATION_ERROR}: {errors}");
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        return StatusCode(201, await _jwtService.BuildAuthResponseAsync(user, MessagesConstant.REGISTRATION_SUCCESS));
    }

    // Renovar token de acceso
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<AuthResDto>>> RefreshToken(RefreshTokenReqDto dto)
    {
        try
        {
            var principal = _jwtService.GetClaimsFromExpiredToken(dto.Token);
            var emailClaim = principal.FindFirstValue(ClaimTypes.Email);

            if (emailClaim is null)
                return Fail(401, MessagesConstant.INVALID_EMAIL_CLAIM);

            var user = await _userManager.FindByEmailAsync(emailClaim);
            if (user is null)
                return Fail(401, MessagesConstant.USER_NOT_FOUND);

            if (user.RefreshToken != dto.RefreshToken)
                return Fail(401, MessagesConstant.INVALID_TOKEN);

            if (user.RefreshTokenExpiration < DateTime.UtcNow)
                return Fail(401, MessagesConstant.TOKEN_EXPIRED);

            return Ok(await _jwtService.BuildAuthResponseAsync(user, MessagesConstant.REFRESH_TOKEN_SUCCESS));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renovar el token para el payload: {Token}", dto.Token);
            return Fail(500, MessagesConstant.REFRESH_TOKEN_ERROR);
        }
    }
}
