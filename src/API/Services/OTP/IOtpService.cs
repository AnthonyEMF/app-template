using API.Database.Entities;

namespace API.Services.OTP
{
    public interface IOtpService
    {
        string GenerateCode();
        string GenerateResetToken();
        Task<UserOtpEntity> CreateOtpAsync(string userId);
        Task InvalidatePreviousOtpsAsync(string userId);
    }
}
