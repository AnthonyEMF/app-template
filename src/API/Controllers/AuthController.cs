using API.Constants;
using API.Database;
using API.Database.Models;
using API.DTOs.Auth.Request;
using API.DTOs.Auth.Response;
using API.DTOs.Shared;
using API.Services.Auth;
using API.Services.Email;
using API.Services.OTP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AuthController(
    SignInManager<UserEntity> _signInManager,
    UserManager<UserEntity> _userManager,
    RoleManager<IdentityRole> _roleManager,
    AppDbContext _context,
    ILogger<AuthController> _logger,
    IJwtService _jwtService,
    IEmailService _emailService,
    IOtpService _otpService
    ) : BaseController
{
    // Iniciar sesión
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<AuthDto>>> Login(LoginDto dto)
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
    public async Task<ActionResult<BaseDto<AuthDto>>> Register(RegisterDto dto)
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
    public async Task<ActionResult<BaseDto<AuthDto>>> RefreshToken(RefreshTokenDto dto)
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

    // Solicitar código OTP
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<object>>> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Ok(200, MessagesConstant.OTP_SEND_SUCCESS, (object)null);  // No revelar si el email existe por seguridad

        await _otpService.InvalidatePreviousOtpsAsync(user.Id);

        var otp = await _otpService.CreateOtpAsync(user.Id);

        try
        {
            await _emailService.SendOtpEmailAsync(user.Email!, user.FirstName, otp.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar OTP al correo {Email}", dto.Email);
            return Fail(500, MessagesConstant.OTP_SEND_ERROR);
        }

        return Ok(200, MessagesConstant.OTP_SEND_SUCCESS, (object)null);
    }

    // Validar código OTP
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<OtpDto>>> VerifyOtp(VerifyOtpDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Fail(401, MessagesConstant.INVALID_OTP);

        var otp = await _context.UsersOtps
            .Where(o => o.UserId == user.Id
                     && o.Code == dto.Code
                     && !o.IsUsed
                     && o.ExpirationDate > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedDate)
            .FirstOrDefaultAsync();

        if (otp is null)
            return Fail(401, MessagesConstant.INVALID_OTP);

        // Marcar OTP como usado
        otp.IsUsed = true;

        // Guardar reset token en el usuario
        var resetToken = _otpService.GenerateResetToken();
        user.ResetToken = resetToken;
        user.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(15);

        await _context.SaveChangesAsync();

        return Ok(200, MessagesConstant.OTP_VERIFIED, new OtpDto { ResetToken = resetToken }); 
    }

    // Restablecer contraseña usando OTP
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseDto<object>>> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Fail(401, MessagesConstant.INVALID_TOKEN);

        if (user.ResetToken != dto.ResetToken)
            return Fail(401, MessagesConstant.INVALID_TOKEN);

        if (user.ResetTokenExpiration < DateTime.UtcNow)
            return Fail(401, MessagesConstant.TOKEN_EXPIRED);

        // Cambiar contraseña con Identity
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Fail(404, $"{MessagesConstant.PASSWORD_RESET_ERROR}: {errors}");
        }

        // Limpiar el reset token
        user.ResetToken = null;
        user.ResetTokenExpiration = null;

        // Actualizar fecha de modificación del usuario
        user.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(200, MessagesConstant.PASSWORD_RESET_SUCCESS, (object)null);
    }
}
