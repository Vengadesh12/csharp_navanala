using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Exceptions;

namespace MyBackend.Api.Middleware
{
    // ==============================================================================
    // TOPIC: Custom middleware & Exception middleware
    // ==============================================================================
    // Custom Middleware in ASP.NET Core:
    //  - Defined with a constructor accepting RequestDelegate 'next' and dependencies.
    //  - Implements 'InvokeAsync(HttpContext context)' to intercept requests/responses.
    //
    // Exception Middleware Role:
    //  - Placed at the very top of the HTTP request pipeline.
    //  - Catches any unhandled exceptions thrown by downstream middleware or controllers.
    //  - Translates domain/application exceptions into standardized JSON ErrorResponse objects.
    //  - Prevents sensitive stack traces and internal server details from leaking to clients.
    // ==============================================================================
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // ==============================================================================
        // TOPIC: Request pipeline execution & Exception catching
        // Invokes downstream pipeline and catches any unhandled exceptions.
        // ==============================================================================
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass control to the next middleware in the request pipeline
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request cancelled by client (navigation or abort).");
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 499; // Client Closed Request
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request processing: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        // ==============================================================================
        // TOPIC: Request/Response manipulation
        // Intercepts the response, sets appropriate HTTP status code, sets Content-Type,
        // and serializes a unified JSON error payload into the HTTP response stream.
        // ==============================================================================
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Response Manipulation: Set JSON response headers and content type
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Success = false,
                Message = exception.Message
            };

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundEx.Message;
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = !string.IsNullOrWhiteSpace(validationEx.Message) && validationEx.Message != "One or more validation failures have occurred."
                        ? validationEx.Message
                        : "Validation failed.";
                    response.Data = validationEx.Errors.ToDictionary(
                        kvp => JsonNamingPolicy.CamelCase.ConvertName(kvp.Key),
                        kvp => kvp.Value
                    );
                    response.Errors = validationEx.Errors.SelectMany(kv => kv.Value).ToList();
                    break;

                case BadRequestException badReqEx:
                case ArgumentException argEx:
                case InvalidOperationException invOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = exception.Message;
                    break;

                case UnauthorizedException unauthEx:
                case UnauthorizedAccessException unauthAccessEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = !string.IsNullOrWhiteSpace(exception.Message) ? exception.Message : "Authentication required. Please provide a valid token.";
                    break;

                case ForbiddenException forbEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Message = !string.IsNullOrWhiteSpace(forbEx.Message) ? forbEx.Message : "Access Denied. You do not have permission to perform this action.";
                    break;

                case ConflictException conflictEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = conflictEx.Message;
                    break;

                case KeyNotFoundException knfEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = knfEx.Message;
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = "A concurrency conflict occurred. The resource has been modified by another operation.";
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx:
                    var innerEx = dbUpdateEx.InnerException;
                    var dbMsg = innerEx?.Message ?? dbUpdateEx.Message;
                    while (innerEx?.InnerException != null)
                    {
                        innerEx = innerEx.InnerException;
                        if (!string.IsNullOrWhiteSpace(innerEx.Message))
                        {
                            dbMsg = innerEx.Message;
                        }
                    }

                    // Check for PostgreSQL unique constraint violation (23505)
                    if (dbMsg.Contains("23505", StringComparison.OrdinalIgnoreCase) ||
                        dbMsg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                        dbMsg.Contains("unique constraint", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        response.Message = "A conflicting resource with the same unique identifier or name already exists.";
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Database operation failed. Please check your request parameters.";
                    }
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An unexpected error occurred.";
                    break;
            }

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
