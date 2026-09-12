using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MyBackend.Api.Middleware;
using MyBackend.Application.Interfaces;
using MyBackend.Configuration;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Services;
using Xunit;

namespace UnitTests.Application
{
    public class SessionExpirationTests
    {
        [Fact]
        public void JwtService_GeneratesToken_With5HourExpiration()
        {
            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "SuperSecretTestingKeyThatIsAtLeast32BytesLong!" },
                { "Jwt:Issuer", "Userspace" },
                { "Jwt:Audience", "Userspace.Web" },
                { "Jwt:ExpiresMinutes", "300" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var jwtService = new JwtService(configuration);

            var user = new UserModel
            {
                Id = 42,
                Name = "Test User",
                Email = "test@example.com",
                RoleId = 1
            };

            var before = DateTime.UtcNow;
            var tokenString = jwtService.GenerateToken(user, "Admin");
            var after = DateTime.UtcNow;

            Assert.False(string.IsNullOrWhiteSpace(tokenString));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            Assert.NotNull(jwt);
            // Verify expiration is approximately 5 hours (300 minutes) from now
            var expectedMinExpiry = before.AddMinutes(299);
            var expectedMaxExpiry = after.AddMinutes(301);

            Assert.True(jwt.ValidTo >= expectedMinExpiry, $"Token ValidTo {jwt.ValidTo} is earlier than expected {expectedMinExpiry}");
            Assert.True(jwt.ValidTo <= expectedMaxExpiry, $"Token ValidTo {jwt.ValidTo} is later than expected {expectedMaxExpiry}");
        }

        [Fact]
        public async Task ActiveSessionValidationMiddleware_TerminatesSession_WhenExceeding5Hours()
        {
            var nextInvoked = false;
            var middleware = new ActiveSessionValidationMiddleware(next: (ctx) =>
            {
                nextInvoked = true;
                return Task.CompletedTask;
            });

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.Request.Path = "/api/users";
            context.Request.Headers["Authorization"] = "Bearer sample_token_123";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "101"),
                new Claim(ClaimTypes.Email, "user101@test.com")
            };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

            // Mock session repository with a session that logged in 5 hours and 10 minutes ago
            var expiredSession = new UserSessionModel
            {
                Id = 999,
                UserId = 101,
                Email = "user101@test.com",
                UserName = "User 101",
                SessionToken = "sample_token_123",
                LoginTime = DateTime.UtcNow.AddMinutes(-310), // 310 minutes > 300 minutes (5 hours)
                IsActive = true,
                DeletedFlag = 1
            };

            var fakeRepo = new FakeSessionRepository(expiredSession);

            await middleware.InvokeAsync(context, fakeRepo);

            Assert.False(nextInvoked, "Pipeline should short-circuit when session has exceeded 5 hours.");
            Assert.Equal(401, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseText = await reader.ReadToEndAsync();

            Assert.Contains("Your session has expired after 5 hours. Please log in again.", responseText);
            Assert.True(fakeRepo.TerminatedSessionId == 999, "Session should be marked terminated in repository.");
        }

        [Fact]
        public async Task ActiveSessionValidationMiddleware_AllowsSession_WhenWithin5Hours()
        {
            var nextInvoked = false;
            var middleware = new ActiveSessionValidationMiddleware(next: (ctx) =>
            {
                nextInvoked = true;
                return Task.CompletedTask;
            });

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.Request.Path = "/api/users";
            context.Request.Headers["Authorization"] = "Bearer sample_token_456";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "102"),
                new Claim(ClaimTypes.Email, "user102@test.com")
            };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

            // Session logged in 2 hours ago (within 5-hour limit)
            var activeSession = new UserSessionModel
            {
                Id = 1000,
                UserId = 102,
                Email = "user102@test.com",
                UserName = "User 102",
                SessionToken = "sample_token_456",
                LoginTime = DateTime.UtcNow.AddHours(-2),
                IsActive = true,
                DeletedFlag = 1
            };

            var fakeRepo = new FakeSessionRepository(activeSession);

            await middleware.InvokeAsync(context, fakeRepo);

            Assert.True(nextInvoked, "Pipeline should continue when session is within 5 hours.");
            Assert.Null(fakeRepo.TerminatedSessionId);
        }

        private class FakeSessionRepository : IUserSessionRepository
        {
            private readonly UserSessionModel? _session;
            public int? TerminatedSessionId { get; private set; }

            public FakeSessionRepository(UserSessionModel? session)
            {
                _session = session;
            }

            public Task<UserSessionModel?> FindActiveSessionByTokenAsync(int userId, string token)
            {
                if (_session != null && _session.UserId == userId && _session.SessionToken == token)
                {
                    return Task.FromResult<UserSessionModel?>(_session);
                }
                return Task.FromResult<UserSessionModel?>(null);
            }

            public Task<bool> TerminateSessionAsync(int sessionId)
            {
                TerminatedSessionId = sessionId;
                if (_session != null && _session.Id == sessionId)
                {
                    _session.IsActive = false;
                    _session.LogoutTime = DateTime.UtcNow;
                }
                return Task.FromResult(true);
            }

            public Task TouchSessionAsync(int sessionId, string clientIp) => Task.CompletedTask;
            public Task AddSessionAsync(UserSessionModel session) => Task.CompletedTask;

            public Task<UserSessionModel> RecordLoginAsync(int userId, string email, string userName, string ipAddress, string? userAgent = null, string? sessionToken = null) => throw new NotImplementedException();
            public Task<bool> RecordLogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null) => throw new NotImplementedException();
            public Task<List<UserSessionModel>> GetUserSessionsAsync(int userId, int limit = 50) => throw new NotImplementedException();
            public Task<List<UserSessionModel>> GetAllRecentSessionsAsync(int limit = 100) => throw new NotImplementedException();
            public Task<List<UserSessionModel>> GetActiveSessionsAsync() => throw new NotImplementedException();
            public Task<(List<UserSessionModel> Items, int TotalCount)> GetPagedSessionsAsync(string? search, string? status, int page, int pageSize) => throw new NotImplementedException();
            public Task<int> TerminateAllUserSessionsAsync(int userId) => throw new NotImplementedException();
            public Task<(int ActiveCount, int TodayLogins, int TodayLogouts, int TotalSessions)> GetActivityStatsAsync() => throw new NotImplementedException();
            public Task<UserSessionModel?> GetSessionByIdAsync(int sessionId) => throw new NotImplementedException();
            public Task<List<UserSessionModel>> GetActiveSessionsForUserAsync(int userId, int? excludeSessionId = null) => throw new NotImplementedException();
            public Task<List<UserSessionModel>> GetActiveSessionsForEmailAsync(string email) => throw new NotImplementedException();
            public Task<int> GetActiveSessionsCountAsync() => throw new NotImplementedException();
            public Task<bool> TerminateSessionWithAuditAsync(int sessionId, int adminUserId) => throw new NotImplementedException();
            public Task<int> ForceLogoutUserWithAuditAsync(int targetUserId, int adminUserId) => throw new NotImplementedException();

            public Task<UserSessionModel?> GetByIdAsync(int id) => Task.FromResult<UserSessionModel?>(null);
            public Task<List<UserSessionModel>> ListAllAsync() => Task.FromResult(new List<UserSessionModel>());
            public Task<List<UserSessionModel>> FindAsync(Expression<Func<UserSessionModel, bool>> predicate) => Task.FromResult(new List<UserSessionModel>());
            public Task<UserSessionModel?> FirstOrDefaultAsync(Expression<Func<UserSessionModel, bool>> predicate) => Task.FromResult<UserSessionModel?>(null);
            public Task<bool> AnyAsync(Expression<Func<UserSessionModel, bool>>? predicate = null) => Task.FromResult(false);
            public Task<int> CountAsync(Expression<Func<UserSessionModel, bool>>? predicate = null) => Task.FromResult(0);
            public Task AddAsync(UserSessionModel entity) => Task.CompletedTask;
            public Task AddRangeAsync(IEnumerable<UserSessionModel> entities) => Task.CompletedTask;
            public void Update(UserSessionModel entity) { }
            public void Delete(UserSessionModel entity) { }
            public void DeleteRange(IEnumerable<UserSessionModel> entities) { }
        }
    }
}
