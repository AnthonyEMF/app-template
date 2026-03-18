using API.Constants;
using API.DTOs.Shared;
using API.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/audit")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AuditController(IAuditService _auditService) : BaseController
    {
        // Obtener todos los logs
        [HttpGet("logs")]
        [Authorize(Roles = $"{RolesConstant.ADMIN}")]
        public async Task<ActionResult<BaseDto<object>>> AllLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _auditService.GetAllLogsAsync(page, pageSize);
            return Ok(200, MessagesConstant.RECORDS_FOUND, logs);
        }

        // Obtener logs por ID de usuario
        [HttpGet("logs/user/{userId}")]
        [Authorize(Roles = $"{RolesConstant.ADMIN}")]
        public async Task<ActionResult<BaseDto<object>>> LogsByUser(
            string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _auditService.GetLogsByUserIdAsync(userId, page, pageSize);
            return Ok(200, MessagesConstant.RECORDS_FOUND, logs);
        }
    }
}
