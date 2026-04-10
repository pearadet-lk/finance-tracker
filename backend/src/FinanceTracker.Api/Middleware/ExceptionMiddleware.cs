using System.Net;
using System.Text.Json;
using FluentValidation;
using Serilog;

namespace FinanceTracker.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation error: {Errors}", ex.Errors);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = 400, errors }));
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning("Unauthorized: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = 401, error = "Unauthorized" }));
        }
        catch (KeyNotFoundException ex)
        {
            Log.Warning("Not found: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = 404, error = ex.Message }));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception for {Path}", context.Request.Path);
            //Log.Error(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = 500, error = "An unexpected error occurred" }));
        }
    }
}
