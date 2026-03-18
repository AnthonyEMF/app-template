using API.Database.Models;
using API.Services.Audit;
using API.Services.Auth;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace API.Middlewares;

public class AuditMiddleware(
    RequestDelegate _next,
    ILogger<AuditMiddleware> _logger,
    IServiceScopeFactory _scopeFactory
    )
{
    // Se excluye GET para evitar logs excesivos
    private static readonly HashSet<string> _trackedMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" }; 

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_trackedMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Interceptar el body de la respuesta para leer el mensaje
        var originalBody = context.Response.Body;
        using var bodyBuffer = new MemoryStream();
        context.Response.Body = bodyBuffer;

        var stopwatch = Stopwatch.StartNew();
        Exception caughtException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            context.Response.StatusCode = 500;
        }
        finally
        {
            stopwatch.Stop();

            // Copiar respuesta al stream original para que llegue al cliente
            bodyBuffer.Seek(0, SeekOrigin.Begin);
            await bodyBuffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        // Leer el cuerpo de la respuesta para extraer el mensaje
        bodyBuffer.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(bodyBuffer).ReadToEndAsync();
        var responseMessage = ExtractMessage(responseBody);

        var log = BuildLog(context, stopwatch.ElapsedMilliseconds, responseMessage, caughtException);

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
                await auditService.InsertLogAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el log en MongoDB");
            }
        });

        // Re-lanzar si hubo excepción no controlada
        if (caughtException is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo
                .Capture(caughtException).Throw();
    }

    // Construir el registro
    private static LogDocument BuildLog(HttpContext context, long durationMs, string responseMessage, Exception exception)
    {
        var statusCode = context.Response.StatusCode;

        // Usuario
        string userId, userName, fullName, role;

        if (context.Items.TryGetValue(JwtService.HttpContextUserKey, out var auditObj)
            && auditObj is LogUser auditUser)
        {
            userId = auditUser.Id;
            userName = auditUser.UserName;
            fullName = auditUser.FullName;
            role = auditUser.Role;
        }
        else
        {
            var user = context.User;
            userId = user.FindFirstValue("userId");
            userName = user.FindFirstValue("userName");
            fullName = user.FindFirstValue("fullName");
            role = user.FindFirstValue(ClaimTypes.Role);
        }

        // Nivel de severidad
        var level = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ when exception is not null => LogLevel.Critical,
            _ => LogLevel.Information,
        };

        return new LogDocument
        {
            Level = level.ToString(),
            Context = new LogContext
            {
                User = new LogUser
                {
                    Id = userId,
                    UserName = userName,
                    FullName = fullName,
                    Role = role,
                },
                Request = new LogRequest
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                },
                Response = new LogResponse
                {
                    Status = statusCode,
                    Success = statusCode < 400,
                    Message = responseMessage,
                    DurationMs = durationMs,
                },
            },
            Error = exception is null ? null : new LogError
            {
                Type = exception.GetType().Name,
                StackTrace = exception.StackTrace,
            },
            Timestamp = DateTime.UtcNow,
        };
    }

    // Extrae el campo "message" del JSON de respuesta
    private static string ExtractMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return string.Empty;

        try
        {
            var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? string.Empty;
        }
        catch { /* respuesta no es JSON */ }

        return string.Empty;
    }
}
