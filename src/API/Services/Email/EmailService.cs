using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace API.Services.Email
{
    public class EmailService(IConfiguration _config, ILogger<EmailService> _logger) : IEmailService
    {
        // Enviar un correo electrónico con el código OTP
        public async Task SendOtpEmailAsync(string toEmail, string userName, string otpCode)
        {
            var emailSettings = _config.GetSection("Email");

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Código de recuperación de contraseña";
            message.Body = new BodyBuilder
            {
                HtmlBody = BuildEmailTemplate(userName, otpCode, int.Parse(emailSettings["OtpExpiration"] ?? "10"))
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                emailSettings["Host"],
                int.Parse(emailSettings["Port"] ?? "587"),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(quit: true);

            _logger.LogInformation("OTP enviado a {Email}", toEmail);
        }

        // Diseño del correo electrónico para enviar códigos OTP
        private static string BuildEmailTemplate(string userName, string otpCode, int expiryMinutes) => $"""
            <!DOCTYPE html>
            <html lang="es">
            <head><meta charset="UTF-8"></head>
            <body style="font-family: Arial, sans-serif; background:#f4f4f4; padding:20px;">
              <div style="max-width:480px; margin:auto; background:#fff;
                          border-radius:8px; padding:32px; box-shadow:0 2px 8px rgba(0,0,0,.1);">

                <h2 style="color:#333; margin-bottom:4px;">Recuperación de contraseña</h2>
                <p style="color:#555;">Hola, <strong>{userName}</strong>.</p>
                <p style="color:#555;">Tu código de verificación es:</p>

                <div style="text-align:center; margin:24px 0;">
                  <span style="font-size:36px; font-weight:bold; letter-spacing:12px;
                               color:#4F46E5; background:#EEF2FF; padding:12px 24px;
                               border-radius:8px;">{otpCode}</span>
                </div>

                <p style="color:#888; font-size:13px;">
                  Este código expira en <strong>{expiryMinutes} minutos</strong>.
                  Si no solicitaste este código, ignora este mensaje.
                </p>
              </div>
            </body>
            </html>
         """;
    }
}
