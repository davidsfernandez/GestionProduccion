using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Helpers;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var title = "Internal Server Error";
        var detail = exception.Message;

        switch (exception)
        {
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Resource Not Found";
                break;
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                title = "Invalid Operation";
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden;
                title = "Forbidden Access";
                detail = "You do not have permission to perform this action.";
                break;
            case GestionProduccion.Domain.Exceptions.DomainConstraintException:
                statusCode = HttpStatusCode.Conflict;
                title = "Domain Constraint Violation";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = _env.IsDevelopment() ? detail : "An error occurred while processing your request.",
            Instance = context.Request.Path
        };

        if (_env.IsDevelopment())
        {
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
    }
}
