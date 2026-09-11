using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyBackend.Api.Middleware
{
    // ==============================================================================
    // TOPIC: Custom middleware & Request/Response manipulation
    // ==============================================================================
    // CorrelationIdMiddleware:
    //  - Demonstrates bi-directional Request and Response manipulation:
    //    1. Request Manipulation: Reads incoming X-Correlation-ID or generates a fresh GUID,
    //       then injects it into HttpContext.Items for downstream loggers and services.
    //    2. Response Manipulation: Injects X-Correlation-ID into outgoing response headers,
    //       along with essential HTTP security hardening headers (nosniff, DENY, etc.).
    // ==============================================================================
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const string CorrelationIdItemKey = "CorrelationId";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ==============================================================================
        // TOPIC: Request/Response manipulation
        // Intercepts context to extract/inject headers before invoking next middleware.
        // ==============================================================================
        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Request Manipulation: Extract incoming correlation ID or generate a new one
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) ||
                string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("D");
            }

            // Store in HttpContext.Items for downstream services, repositories, and loggers
            context.Items[CorrelationIdItemKey] = correlationId.ToString();

            // 2. Response Manipulation: Attach correlation tracking and security headers to response
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId.ToString();
            }

            if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            }

            if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
            }

            if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
            {
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            }

            if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            }

            // Continue to next middleware in request pipeline
            await _next(context);
        }
    }
}
