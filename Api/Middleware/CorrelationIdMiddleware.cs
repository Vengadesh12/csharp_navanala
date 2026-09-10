using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyBackend.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const string CorrelationIdItemKey = "CorrelationId";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract or generate correlation ID
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) ||
                string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("D");
            }

            // Store in HttpContext.Items for downstream services and middleware
            context.Items[CorrelationIdItemKey] = correlationId.ToString();

            // Set response header and security headers
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

            await _next(context);
        }
    }
}
