using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyBackend.Api.Middleware
{
    // ==============================================================================
    // TOPIC: Logging middleware & Custom middleware
    // ==============================================================================
    // Logging Middleware:
    //  - Intercepts all incoming HTTP requests and outgoing HTTP responses.
    //  - Uses a high-resolution Stopwatch to capture exact execution duration.
    //  - Sanitizes query strings to redact confidential parameters (passwords, tokens, keys).
    //  - Tags logs with the unique Correlation ID and User Identity for end-to-end tracing.
    // ==============================================================================
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        private static readonly string[] SensitiveQueryKeys = new[]
        {
            "password", "pass", "pwd", "token", "secret", "authorization",
            "otp", "code", "apikey", "access_token", "refreshtoken", "key"
        };

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // ==============================================================================
        // TOPIC: Request/Response manipulation (Inspection & Telemetry)
        // Inspects the request path and query before execution;
        // reads the response status code and execution duration in the finally block.
        // ==============================================================================
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // Extract Correlation ID from context items or header
            var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdItemKey]?.ToString()
                ?? context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader].ToString();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = context.TraceIdentifier;
            }

            // Request Manipulation/Sanitization: Redact sensitive query parameters
            var safeQuery = GetSanitizedQueryString(request.QueryString);

            try
            {
                // Continue downstream request pipeline
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Capture authenticated user context (if present)
                var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.User?.FindFirstValue("sub")
                    ?? "Anonymous";

                // Response Inspection: Read downstream HTTP status code
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "HTTP {Method} {Path}{Query} returned {StatusCode} in {ElapsedMs} ms [CorrelationId: {CorrelationId}, UserId: {UserId}]",
                        request.Method,
                        request.Path.Value,
                        safeQuery,
                        statusCode,
                        elapsedMs,
                        correlationId,
                        userId
                    );
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP {Method} {Path}{Query} returned {StatusCode} in {ElapsedMs} ms [CorrelationId: {CorrelationId}, UserId: {UserId}]",
                        request.Method,
                        request.Path.Value,
                        safeQuery,
                        statusCode,
                        elapsedMs,
                        correlationId,
                        userId
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path}{Query} returned {StatusCode} in {ElapsedMs} ms [CorrelationId: {CorrelationId}, UserId: {UserId}]",
                        request.Method,
                        request.Path.Value,
                        safeQuery,
                        statusCode,
                        elapsedMs,
                        correlationId,
                        userId
                    );
                }
            }
        }

        private static string GetSanitizedQueryString(QueryString queryString)
        {
            if (!queryString.HasValue) return string.Empty;

            try
            {
                var queryDict = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString.Value);
                var sanitizedParams = queryDict.Select(kvp =>
                {
                    var isSensitive = SensitiveQueryKeys.Any(k => kvp.Key.Contains(k, StringComparison.OrdinalIgnoreCase));
                    return isSensitive ? $"{kvp.Key}=[REDACTED]" : $"{kvp.Key}={kvp.Value}";
                });

                return "?" + string.Join("&", sanitizedParams);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
