using API.Constants;
using API.Database;
using API.Database.Models;
using API.DTOs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Controllers;

[Route("api/audit")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AuditController(AuditDbContext _auditDb) : BaseController
{
    // Obtener todos los logs de la API
    [HttpGet("logs")]
    [Authorize(Roles = RolesConstant.ADMIN)]
    public async Task<ActionResult<BaseDto<object>>> ApiLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string searchTerm = null,
        [FromQuery] string method = null,
        [FromQuery] bool? success = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null
        )
    {
        var builder = Builders<LogDocument>.Filter;
        var filterDefs = new List<FilterDefinition<LogDocument>>();

        // Filtros 
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Búsqueda por coincidencia parcial en userName, fullName o path
            var regex = new BsonRegularExpression(searchTerm, "i");
            filterDefs.Add(builder.Or(
                builder.Regex("context.user.userName", regex),
                builder.Regex("context.user.fullName", regex),
                builder.Regex("context.request.path", regex)
            ));
        }

        // Por método HTTP (POST, PUT, PATCH, DELETE)
        if (!string.IsNullOrWhiteSpace(method))
            filterDefs.Add(builder.Eq("context.request.method", method.ToUpper()));

        // Por éxito o error de la respuesta
        if (success == true || success == false)
            filterDefs.Add(builder.Eq("context.response.success", success));

        // Por rango de fechas
        if (from.HasValue)
            filterDefs.Add(builder.Gte(l => l.Timestamp, from.Value.ToUniversalTime()));

        if (to.HasValue)
            filterDefs.Add(builder.Lte(l => l.Timestamp, to.Value.ToUniversalTime()));

        var combinedFilter = filterDefs.Count > 0
            ? builder.And(filterDefs)
            : builder.Empty;

        // Paginación
        var totalItems = (int)await _auditDb.ApiLogs.CountDocumentsAsync(combinedFilter);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await _auditDb.ApiLogs
            .Find(combinedFilter)
            .SortByDescending(l => l.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var result = new PaginationDto<List<LogDocument>>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages,
            Items = items,
        };

        return Ok(200, MessagesConstant.RECORDS_FOUND, result);
    }
}
