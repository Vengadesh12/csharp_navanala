using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MyBackend.Api.Middleware;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Exceptions;
using Xunit;

namespace UnitTests.Application
{
    public class MiddlewareTests
    {
        [Fact]
        public async Task CorrelationIdMiddleware_GeneratesNewId_WhenNoneProvided()
        {
            var context = new DefaultHttpContext();
            var middleware = new CorrelationIdMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.True(context.Response.Headers.ContainsKey("X-Correlation-ID"));
            var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
            Assert.False(string.IsNullOrWhiteSpace(correlationId));

            // Verify security headers
            Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"].ToString());
            Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"].ToString());
            Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"].ToString());
        }

        [Fact]
        public async Task CorrelationIdMiddleware_PreservesExistingCorrelationId()
        {
            var context = new DefaultHttpContext();
            var expectedId = "custom-client-trace-12345";
            context.Request.Headers["X-Correlation-ID"] = expectedId;

            var middleware = new CorrelationIdMiddleware(next: (innerHttpContext) => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal(expectedId, context.Response.Headers["X-Correlation-ID"].ToString());
        }

        [Fact]
        public async Task ExceptionMiddleware_HandlesConflictException_As409()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var logger = NullLogger<ExceptionMiddleware>.Instance;
            var middleware = new ExceptionMiddleware(
                next: (ctx) => throw new ConflictException("Email address already exists in the system."),
                logger: logger);

            await middleware.InvokeAsync(context);

            Assert.Equal(409, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseJson = await reader.ReadToEndAsync();
            var result = JsonSerializer.Deserialize<ErrorResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Email address already exists in the system.", result.Message);
        }

        [Fact]
        public async Task ExceptionMiddleware_HandlesValidationException_As400_WithDataDictionary()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var logger = NullLogger<ExceptionMiddleware>.Instance;
            var middleware = new ExceptionMiddleware(
                next: (ctx) => throw new ValidationException("Invalid input", new System.Collections.Generic.Dictionary<string, string[]>
                {
                    { "Email", new[] { "Invalid format" } }
                }),
                logger: logger);

            await middleware.InvokeAsync(context);

            Assert.Equal(400, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseJson = await reader.ReadToEndAsync();
            var result = JsonSerializer.Deserialize<ErrorResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task ExceptionMiddleware_UnhandledException_Returns500_WithoutStackTraceInProduction()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var logger = NullLogger<ExceptionMiddleware>.Instance;
            var middleware = new ExceptionMiddleware(
                next: (ctx) => throw new Exception("Sensitive internal database connection error"),
                logger: logger);

            await middleware.InvokeAsync(context);

            Assert.Equal(500, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseJson = await reader.ReadToEndAsync();

            Assert.DoesNotContain("Sensitive internal database connection error", responseJson);
            Assert.DoesNotContain("at UnitTests", responseJson); // No stack trace
        }
    }
}
