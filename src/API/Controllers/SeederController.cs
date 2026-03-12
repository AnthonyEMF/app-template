using API.Constants;
using API.DTOs.Shared;
using API.Services.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/seeder")]
    [ApiController]
    public class SeederController(ISeedService _seedService, IWebHostEnvironment _env) : BaseController
    {
        // Cargar usuarios de prueba (users.json)
        [HttpPost("users")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseDto<object>>> SeedUsers()
        {
            if (!_env.IsDevelopment())
                return Fail(403, MessagesConstant.INVALID_ENV);

            await _seedService.LoadUsersAsync();

            return Ok(201, MessagesConstant.SEED_SUCCESS, (object)null);
        }
    }
}
