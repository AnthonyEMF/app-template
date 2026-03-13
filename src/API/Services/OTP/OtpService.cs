using API.Database;
using API.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Services.OTP
{
    public class OtpService(AppDbContext _context, IConfiguration _config) : IOtpService
    {
        // Generar código de 6 dígitos
        public string GenerateCode()
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return code.ToString("D6");  // 000000 - 999999
        }

        // Generar token para autorizar el reset de contraseña
        public string GenerateResetToken()
        {
            var bytes = new byte[48];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        // Invalidar códigos OTPs anteriores
        public async Task InvalidatePreviousOtpsAsync(string userId)
        {
            await _context.UsersOtps
                .Where(o => o.UserId == userId && !o.IsUsed && o.ExpirationDate > DateTime.UtcNow)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.IsUsed, true));
        }

        // Crear nuevo código OTP
        public async Task<UserOtpEntity> CreateOtpAsync(string userId)
        {
            var expiryMinutes = int.Parse(_config["Email:OtpExpiration"] ?? "10");

            var otp = new UserOtpEntity
            {
                UserId = userId,
                Code = GenerateCode(),
                ExpirationDate = DateTime.UtcNow.AddMinutes(expiryMinutes),
                CreatedDate = DateTime.UtcNow,
            };

            _context.UsersOtps.Add(otp);
            await _context.SaveChangesAsync();
            return otp;
        }
    }
}
