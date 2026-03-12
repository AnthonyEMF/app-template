using API.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    // Error genérico (Bad Request, Unauthorized, NotFound, Forbidden, etc)
    protected ObjectResult Fail(int statusCode, string message) =>
        StatusCode(statusCode, new BaseDto<object>
        {
            StatusCode = statusCode,
            Status = false,
            Message = message,
        });

    // Respuesta exitosa (Ok, Created, NoContent, etc)
    protected ObjectResult Ok<T>(int statusCode, string message, T data) =>
        StatusCode(statusCode, new BaseDto<T>
        {
            StatusCode = statusCode,
            Status = true,
            Message = message,
            Data = data,
        });
}