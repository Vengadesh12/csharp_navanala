using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyBackend.Api.Middleware
{
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

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdItemKey]?.ToString()
                ?? context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader].ToString();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = context.TraceIdentifier;
            }

            var safeQuery = GetSanitizedQueryString(request.QueryString);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.User?.FindFirstValue("sub")
                    ?? "Anonymous";

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
