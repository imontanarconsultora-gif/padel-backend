using System.Text.Json;
using PadelApi.Domain.Exceptions;

namespace PadelApi.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (PadelException ex)
        {
            ctx.Response.StatusCode = ex.StatusCode;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                ok = false,
                mensaje = ex.Message,
                codigo = ex.Codigo
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no manejado");
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                ok = false,
                mensaje = "Error interno del servidor.",
                codigo = "error_interno"
            }));
        }
    }
}
