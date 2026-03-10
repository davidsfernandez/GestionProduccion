/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using System.Net;
using System.Text.Json;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Resources;

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
        var detail = exception.Message;
        var localizedMessage = Portuguese.Err_Generic;

        switch (exception)
        {
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                localizedMessage = Portuguese.Err_NotFound;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden;
                localizedMessage = Portuguese.Err_Unauthorized;
                break;
            case GestionProduccion.Domain.Exceptions.DomainConstraintException:
                statusCode = HttpStatusCode.Conflict;
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.FailureResult(localizedMessage, _env.IsDevelopment() ? new List<string> { detail, exception.StackTrace ?? "" } : new List<string>());

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
