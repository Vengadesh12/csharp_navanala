using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Services;
using MyBackend.Domain.Models;
using Xunit;

namespace MyBackend.UnitTests.Application
{
    public class AuthServiceTests
    {
        private readonly FakeUserRepository _userRepository;
        private readonly FakeUnitOfWork _unitOfWork;
        private readonly FakeJwtService _jwtService;
        private readonly FakeOtpService _otpService;
        private readonly FakeEmailService _emailService;
        private readonly IPasswordHasher<UserModel> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthServiceTests()
        {
            _userRepository = new FakeUserRepository();
            _unitOfWork = new FakeUnitOfWork(_userRepository);
            _jwtService = new FakeJwtService();
            _otpService = new FakeOtpService();
            _emailService = new FakeEmailService();
            _passwordHasher = new PasswordHasher<UserModel>();
            _configuration = new FakeConfiguration();
            _logger = NullLogger<AuthService>.Instance;
        }

        private class FakeConfiguration : IConfiguration
        {
            public string? this[string key] { get => null; set { } }
            public IEnumerable<IConfigurationSection> GetChildren() => [];
            public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken() => null!;
            public IConfigurationSection GetSection(string key) => null!;
        }

        private AuthService CreateAuthService(IPasswordHasher<UserModel>? hasher = null)
        {
            return new AuthService(
                _userRepository,
                _unitOfWork,
                _configuration,
                _emailService,
                _otpService,
                _jwtService,
                hasher ?? _passwordHasher,
                _logger
            );
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessfulLoginResponse()
        {
            // Arrange
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 10,
                Name = "John Doe",
                Email = "john@example.com",
                RoleId = 1,
                DesignationId = 5,
                Phone = "1234567890",
                Age = 30,
                Address = "123 Main St",
                DeletedFlag = 1
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "SecureP@ssword123");
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new LoginRequest
            {
                Email = "john@example.com",
                Password = "SecureP@ssword123"
            };

            // Act
            var response = await authService.LoginAsync(request, "192.168.1.50", "Mozilla/5.0");

            // Assert
            Assert.True(response.Success);
            Assert.False(response.RequiresTwoFactor);
            Assert.Equal("Login successful.", response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(10, response.Data.Id);
            Assert.Equal("John Doe", response.Data.Name);
            Assert.Equal("john@example.com", response.Data.Email);
            Assert.Equal("mock_jwt_token", response.Data.Token);
            Assert.Equal("1234567890", response.Data.Phone);
            Assert.Equal(30, response.Data.Age);
            Assert.Equal("123 Main St", response.Data.Address);
            Assert.Equal("Standard Role", response.Data.RoleName);
            Assert.Equal("Software Engineer", response.Data.DesignationName);
            Assert.Equal("Engineering", response.Data.DepartmentName);
            Assert.Contains("dashboard.view", response.Data.Permissions);
            Assert.Contains("Dashboard", response.Data.MenuNames);
            Assert.Single(response.Data.Menus);
            Assert.Equal("Dashboard", response.Data.Menus[0].Label);
            Assert.Equal("Dashboard", response.Data.Menus[0].Name);
            Assert.Equal("Dashboard", response.Data.Menus[0].MenuName);
            Assert.Equal(1, _unitOfWork.SessionsRecorded);
            Assert.Equal(1, _unitOfWork.AuditLogsRecorded);
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var authService = CreateAuthService();
            var request = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "AnyPassword"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(request));
            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 11,
                Name = "Jane Doe",
                Email = "jane@example.com",
                DeletedFlag = 1
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "CorrectP@ssword1");
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new LoginRequest
            {
                Email = "jane@example.com",
                Password = "WrongPassword999"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(request));
            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_InsecurePlaintextHashComparison_IsRejected()
        {
            // Verify that plaintext matching of stored password hash is NOT accepted
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 12,
                Name = "Target User",
                Email = "target@example.com",
                DeletedFlag = 1
            };
            var realHash = _passwordHasher.HashPassword(user, "SecretPassword1!");
            user.PasswordHash = realHash;
            _userRepository.UsersByEmail[user.Email] = user;

            // Attacker provides the raw hash string directly as the password
            var request = new LoginRequest
            {
                Email = "target@example.com",
                Password = realHash
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_HardcodedDemoPasswordBackdoor_IsRejected()
        {
            // Verify that previous backdoor for admin@example.com with 'admin@123' is removed
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 1,
                Name = "Administrator",
                Email = "admin@example.com",
                RoleId = 2,
                DeletedFlag = 1
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "VeryDifferentComplexP@ssword#2026");
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new LoginRequest
            {
                Email = "admin@example.com",
                Password = "admin@123" // Previous hardcoded backdoor password
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_DeactivatedAccount_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 13,
                Name = "Deactivated User",
                Email = "deactivated@example.com",
                DeletedFlag = 0 // Deactivated
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "ValidPassword1!");
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new LoginRequest
            {
                Email = "deactivated@example.com",
                Password = "ValidPassword1!"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(request));
            Assert.Contains("deactivated", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData("email@test.com", "")]
        [InlineData("   ", "password")]
        [InlineData("email@test.com", "   ")]
        public async Task LoginAsync_EmptyEmailOrPassword_ThrowsArgumentException(string email, string password)
        {
            var authService = CreateAuthService();
            var request = new LoginRequest { Email = email, Password = password };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_TwoFactorEnabled_ReturnsRequiresTwoFactorResponse()
        {
            // Arrange
            var authService = CreateAuthService();
            _unitOfWork.Settings["two_factor_auth"] = "true";

            var user = new UserModel
            {
                Id = 14,
                Name = "2FA User",
                Email = "twofactor@example.com",
                DeletedFlag = 1
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "P@ssword123!");
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new LoginRequest
            {
                Email = "twofactor@example.com",
                Password = "P@ssword123!"
            };

            // Act
            var response = await authService.LoginAsync(request);

            // Assert
            Assert.True(response.Success);
            Assert.True(response.RequiresTwoFactor);
            Assert.Contains("Two-Factor Authentication is enabled", response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal("twofactor@example.com", response.Data.Email);
            Assert.Equal("2FA User", response.Data.Name);
            Assert.Equal(1, _emailService.Sent2FaEmailsCount);
        }

        [Fact]
        public async Task LoginAsync_PasswordRehashNeeded_UpdatesStoredPasswordHash()
        {
            // Arrange: fake hasher that returns SuccessRehashNeeded
            var mockHasher = new RehashPromptingHasher();
            var authService = CreateAuthService(mockHasher);

            var user = new UserModel
            {
                Id = 15,
                Name = "Rehash User",
                Email = "rehash@example.com",
                PasswordHash = "OldWeakHashFormat",
                DeletedFlag = 1
            };
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new LoginRequest
            {
                Email = "rehash@example.com",
                Password = "ValidPassword123"
            };

            // Act
            var response = await authService.LoginAsync(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(15, _userRepository.LastUpdatedUserId);
            Assert.StartsWith("rehashed_", _userRepository.LastUpdatedHash);
        }

        [Fact]
        public async Task ForgotPasswordAsync_ValidEmail_DispatchesEmailSuccessfully()
        {
            // Arrange
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 20,
                Name = "Jane Doe",
                Email = "jane@example.com",
                DeletedFlag = 1
            };
            _userRepository.UsersByEmail[user.Email] = user;

            var request = new ForgotPasswordRequest { Email = "jane@example.com" };

            // Act
            var response = await authService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(1, _emailService.SentPasswordResetEmailsCount);
            Assert.Contains("6-digit verification OTP has been sent", response.Message);
        }

        [Fact]
        public async Task ForgotPasswordAsync_EmailFailsInDevMode_ReturnsGracefulResponseWithOtp()
        {
            // Arrange
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 21,
                Name = "John Developer",
                Email = "dev@example.com",
                DeletedFlag = 1
            };
            _userRepository.UsersByEmail[user.Email] = user;
            _emailService.ShouldThrow = true;

            var request = new ForgotPasswordRequest { Email = "dev@example.com" };

            // Act
            var response = await authService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(response.Success);
            Assert.Contains("[Dev Mode]", response.Message);
            Assert.Contains("123456", response.Message);
        }

        [Fact]
        public async Task Resend2FaOtpAsync_EmailFailsInDevMode_ReturnsGracefulResponseWithOtp()
        {
            // Arrange
            var authService = CreateAuthService();
            var user = new UserModel
            {
                Id = 22,
                Name = "Alice TwoFa",
                Email = "alice@example.com",
                DeletedFlag = 1
            };
            _userRepository.UsersByEmail[user.Email] = user;
            _emailService.ShouldThrow = true;

            var request = new Resend2FaOtpRequest { Email = "alice@example.com" };

            // Act
            var response = await authService.Resend2FaOtpAsync(request);

            // Assert
            Assert.True(response.Success);
            Assert.Contains("[Dev Mode]", response.Message);
            Assert.Contains("123456", response.Message);
        }

        private class RehashPromptingHasher : IPasswordHasher<UserModel>
        {
            public string HashPassword(UserModel user, string password) => $"rehashed_{password}";

            public PasswordVerificationResult VerifyHashedPassword(UserModel user, string hashedPassword, string providedPassword)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }
        }

        // ====================================================================
        // Test doubles (Fakes)
        // ====================================================================

        private class FakeRepository<T> : IRepository<T> where T : class
        {
            public virtual Task<T?> GetByIdAsync(int id) => Task.FromResult<T?>(null);
            public virtual Task<List<T>> ListAllAsync() => Task.FromResult(new List<T>());
            public virtual Task<List<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) => Task.FromResult(new List<T>());
            public virtual Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) => Task.FromResult<T?>(null);
            public virtual Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null) => Task.FromResult(false);
            public virtual Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null) => Task.FromResult(0);
            public virtual Task AddAsync(T entity) => Task.CompletedTask;
            public virtual Task AddRangeAsync(IEnumerable<T> entities) => Task.CompletedTask;
            public virtual void Update(T entity) { }
            public virtual void Delete(T entity) { }
            public virtual void DeleteRange(IEnumerable<T> entities) { }
        }

        private class FakeUserRepository : FakeRepository<UserModel>, IUserRepository
        {
            public Dictionary<string, UserModel> UsersByEmail = new(StringComparer.OrdinalIgnoreCase);
            public int LastUpdatedUserId { get; private set; }
            public string LastUpdatedHash { get; private set; } = string.Empty;

            public Task<UserModel?> GetByEmailAsync(string email)
            {
                UsersByEmail.TryGetValue(email, out var user);
                return Task.FromResult(user);
            }

            public Task<UserLoginDetailsModel?> GetLoginUserDetailsByEmailAsync(string email)
            {
                if (UsersByEmail.TryGetValue(email, out var user))
                {
                    return Task.FromResult<UserLoginDetailsModel?>(new UserLoginDetailsModel
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        RoleId = user.RoleId,
                        RoleName = "Standard Role",
                        DesignationId = user.DesignationId,
                        DesignationName = "Software Engineer",
                        DepartmentName = "Engineering",
                        Phone = user.Phone,
                        Age = user.Age,
                        Address = user.Address,
                        ProfileImage = user.ProfileImage,
                        DeletedFlag = user.DeletedFlag,
                        IsFirstLogin = user.IsFirstLogin,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        PermissionsCsv = "dashboard.view,users.view",
                        MenuNamesCsv = "Dashboard"
                    });
                }
                return Task.FromResult<UserLoginDetailsModel?>(null);
            }

            public Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash)
            {
                LastUpdatedUserId = userId;
                LastUpdatedHash = newPasswordHash;
                return Task.FromResult(true);
            }

            public Task<List<string>> GetUserPermissionKeysAsync(int userId, int? roleId = null, int? designationId = null) =>
                Task.FromResult(new List<string> { "dashboard.view", "users.view" });

            public Task<UserModel?> GetUserByIdAsync(int id) => Task.FromResult<UserModel?>(null);
            public Task<List<UserModel>> GetAllUsersAsync() => Task.FromResult(new List<UserModel>());
            public Task<bool> SetDeletedFlagAsync(int id, int deletedFlag) => Task.FromResult(true);
            public Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys) => Task.FromResult(true);
            public Task<Dictionary<int, string>> GetActiveRolesLookupAsync() => Task.FromResult(new Dictionary<int, string>());
            public Task<Dictionary<int, string>> GetActiveDesignationsLookupAsync() => Task.FromResult(new Dictionary<int, string>());
            public Task<string?> GetRoleNameByIdAsync(int roleId) => Task.FromResult<string?>("Standard Role");
            public Task<string?> GetDesignationNameByIdAsync(int designationId) => Task.FromResult<string?>("Software Engineer");
            public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null) => Task.FromResult(false);
            public Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null) => Task.FromResult(false);
            public Task<int> GetActiveUsersCountAsync() => Task.FromResult(1);
            public Task<int> GetUsersWithRoleCountAsync() => Task.FromResult(1);
            public Task<List<string>> GetUserPermissionKeysForProfileAsync(int roleId, int designationId) => Task.FromResult(new List<string>());
            public Task<Dictionary<int, string>> GetUserRoleMapAsync() => Task.FromResult(new Dictionary<int, string>());
        }

        private class FakeUnitOfWork : IUnitOfWork
        {
            public IUserRepository Users { get; }
            public IRoleRepository Roles { get; } = new FakeRoleRepository();
            public IDepartmentRepository Departments { get; } = new FakeDepartmentRepository();
            public IDesignationRepository Designations { get; } = new FakeDesignationRepository();
            public IPermissionRepository Permissions => throw new NotImplementedException();
            public ISettingRepository SystemSettings { get; }
            public IMenuRepository Menus { get; } = new FakeMenuRepository();
            public IUserSessionRepository Sessions { get; }
            public IScheduleRepository Schedules => throw new NotImplementedException();
            public IReportRepository Reports => throw new NotImplementedException();
            public IAuditLogRepository AuditLogs { get; }
            public IApprovalRepository Approvals => throw new NotImplementedException();
            public IAccessRequestRepository AccessRequests => throw new NotImplementedException();
            public IPurchaseRepository Purchases => throw new NotImplementedException();
            public IInvoiceRepository Invoices => throw new NotImplementedException();
            public IDashboardRepository Dashboard => throw new NotImplementedException();
            public IProjectRepository Projects => throw new NotImplementedException();
            public IProjectCategoryRepository ProjectCategories => throw new NotImplementedException();

            public int SessionsRecorded = 0;
            public int AuditLogsRecorded = 0;
            public Dictionary<string, string> Settings = new(StringComparer.OrdinalIgnoreCase);

            public FakeUnitOfWork(IUserRepository users)
            {
                Users = users;
                SystemSettings = new FakeSettingRepository(Settings);
                Sessions = new FakeUserSessionRepository(() => SessionsRecorded++);
                AuditLogs = new FakeAuditLogRepository(() => AuditLogsRecorded++);
            }

            public IRepository<T> Repository<T>() where T : class => throw new NotImplementedException();
            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
            public Task<IDbTransaction> BeginTransactionAsync() => throw new NotImplementedException();
            public Task CommitTransactionAsync() => Task.CompletedTask;
            public Task RollbackTransactionAsync() => Task.CompletedTask;
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }

        private class FakeRoleRepository : FakeRepository<RoleModel>, IRoleRepository
        {
            public override Task<RoleModel?> GetByIdAsync(int id) => Task.FromResult<RoleModel?>(new RoleModel { Id = id, Name = "Standard Role", DeletedFlag = 1 });
            public Task<List<RoleModel>> GetActiveRolesAsync() => Task.FromResult(new List<RoleModel>());
            public Task<RoleModel?> GetActiveRoleByIdAsync(int id) => Task.FromResult<RoleModel?>(null);
            public Task<bool> SetDeletedFlagAsync(int id, int deletedFlag) => Task.FromResult(true);
            public Task<Dictionary<int, string>> GetRoleNameDictionaryAsync() => Task.FromResult(new Dictionary<int, string>());
        }

        private class FakeDepartmentRepository : FakeRepository<DepartmentModel>, IDepartmentRepository
        {
            public override Task<DepartmentModel?> GetByIdAsync(int id) => Task.FromResult<DepartmentModel?>(new DepartmentModel { Id = id, Name = "Engineering", DeletedFlag = 1 });
            public Task<List<DepartmentModel>> GetActiveDepartmentsWithDesignationsAsync() => Task.FromResult(new List<DepartmentModel>());
            public Task<DepartmentModel?> GetActiveDepartmentByIdAsync(int id) => Task.FromResult<DepartmentModel?>(null);
            public Task<Dictionary<int, string>> GetDepartmentNameDictionaryAsync() => Task.FromResult(new Dictionary<int, string>());
            public Task<bool> DepartmentExistsByNameAsync(string name, int? excludeId = null) => Task.FromResult(false);
        }

        private class FakeDesignationRepository : FakeRepository<DesignationModel>, IDesignationRepository
        {
            public override Task<DesignationModel?> GetByIdAsync(int id) => Task.FromResult<DesignationModel?>(new DesignationModel { Id = id, Name = "Software Engineer", DepartmentId = 1, DeletedFlag = 1 });
            public Task<List<DesignationModel>> GetActiveDesignationsAsync() => Task.FromResult(new List<DesignationModel>());
            public Task<DesignationModel?> GetActiveDesignationByIdAsync(int id) => Task.FromResult<DesignationModel?>(null);
            public Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync() => Task.FromResult(new Dictionary<int, string>());
            public Task<bool> DesignationExistsByNameAsync(string name, int? excludeId = null) => Task.FromResult(false);
            public Task<string?> GetDepartmentNameByIdAsync(int departmentId) => Task.FromResult<string?>("Engineering");
            public Task<bool> SetDeletedFlagAsync(int id, int deletedFlag) => Task.FromResult(true);
            public Task<List<DesignationModel>> GetDesignationsByIdsAsync(IEnumerable<int> ids) => Task.FromResult(new List<DesignationModel>());
            public Task<List<DesignationModel>> GetDesignationsByDepartmentIdAsync(int departmentId) => Task.FromResult(new List<DesignationModel>());
        }

        private class FakeSettingRepository : ISettingRepository
        {
            private readonly Dictionary<string, string> _settings;
            public FakeSettingRepository(Dictionary<string, string> settings) => _settings = settings;

            public Task<string?> GetSettingValueAsync(string key)
            {
                _settings.TryGetValue(key, out var val);
                return Task.FromResult<string?>(val);
            }

            public Task<(List<SystemSettingModel> Settings, List<SettingCategoryModel> Categories, Dictionary<string, int> SettingCounts, int TotalSettings, string? TwoFactorValue, int AlertChannels, string? SessionTimeout)> GetSettingsOverviewDataAsync(string? category, string? search)
                => Task.FromResult((new List<SystemSettingModel>(), new List<SettingCategoryModel>(), new Dictionary<string, int>(), 0, (string?)"false", 0, (string?)"30m"));

            public Task<(List<SettingCategoryModel> Categories, Dictionary<string, int> SettingCounts)> GetCategoriesWithCountsAsync()
                => Task.FromResult((new List<SettingCategoryModel>(), new Dictionary<string, int>()));

            public Task<bool> CategoryExistsByNameAsync(string name, int? excludeId = null) => Task.FromResult(false);
            public Task<int> CreateCategoryAsync(string name, string description, string icon, string createdBy) => Task.FromResult(1);
            public Task<SettingCategoryModel?> GetCategoryByIdAsync(int id) => Task.FromResult<SettingCategoryModel?>(null);
            public Task<int> GetCategorySettingCountAsync(string categoryName) => Task.FromResult(0);
            public Task<bool> UpdateCategoryAsync(int id, string name, string description, string icon) => Task.FromResult(true);
            public Task<bool> SoftDeleteCategoryAsync(int id) => Task.FromResult(true);
            public Task<bool> BulkUpdateSettingsAsync(IDictionary<string, string> settings, string updatedBy) => Task.FromResult(true);
            public Task<bool> SettingExistsByKeyAsync(string key) => Task.FromResult(false);
            public Task<int> CreateSettingAsync(string key, string value, string category, string description, string dataType, string createdBy) => Task.FromResult(1);
            public Task<SystemSettingModel?> GetSettingByIdAsync(int id) => Task.FromResult<SystemSettingModel?>(null);
            public Task<bool> UpdateSettingAsync(int id, string key, string value, string category, string description, string dataType, string updatedBy) => Task.FromResult(true);
            public Task<bool> DeleteSettingAsync(int id) => Task.FromResult(true);
        }

        private class FakeMenuRepository : IMenuRepository
        {
            public Task<List<MenuModel>> GetAllActiveMenusAsync() => Task.FromResult(new List<MenuModel>());
            public Task<List<string>> GetAllActiveMenuNamesAsync() => Task.FromResult(new List<string> { "Dashboard" });
            public Task<List<MenuModel>> GetUserMenusAsync(int roleId, int designationId, int? userId = null) => Task.FromResult(new List<MenuModel>
            {
                new MenuModel { Id = 1, MenuKey = "dashboard", Label = "Dashboard", Route = "/dashboard" }
            });
            public Task<List<string>> GetUserMenuNamesAsync(int roleId, int designationId, int? userId = null) => Task.FromResult(new List<string> { "Dashboard" });
            public Task<IEnumerable<MenuModel>> GetAllAsync() => Task.FromResult<IEnumerable<MenuModel>>([]);
            public Task<MenuModel?> GetByIdAsync(int id) => Task.FromResult<MenuModel?>(null);
            public Task AddAsync(MenuModel entity) => Task.CompletedTask;
            public Task UpdateAsync(MenuModel entity) => Task.CompletedTask;
            public Task DeleteAsync(MenuModel entity) => Task.CompletedTask;
            public Task<int> CountAsync() => Task.FromResult(1);
        }

        private class FakeUserSessionRepository : IUserSessionRepository
        {
            private readonly Action _onRecordLogin;
            public FakeUserSessionRepository(Action onRecordLogin) => _onRecordLogin = onRecordLogin;

            public Task<UserSessionModel> RecordLoginAsync(int userId, string email, string userName, string ipAddress, string? userAgent = null, string? sessionToken = null)
            {
                _onRecordLogin();
                return Task.FromResult(new UserSessionModel
                {
                    UserId = userId,
                    Email = email,
                    UserName = userName,
                    IpAddress = ipAddress,
                    SessionToken = sessionToken
                });
            }

            public Task<bool> RecordLogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null) => Task.FromResult(true);
            public Task<List<UserSessionModel>> GetUserSessionsAsync(int userId, int limit = 50) => Task.FromResult(new List<UserSessionModel>());
            public Task<List<UserSessionModel>> GetAllRecentSessionsAsync(int limit = 100) => Task.FromResult(new List<UserSessionModel>());
            public Task<List<UserSessionModel>> GetActiveSessionsAsync() => Task.FromResult(new List<UserSessionModel>());
            public Task<(List<UserSessionModel> Items, int TotalCount)> GetPagedSessionsAsync(string? search, string? status, int page, int pageSize) => Task.FromResult((new List<UserSessionModel>(), 0));
            public Task<bool> TerminateSessionAsync(int sessionId) => Task.FromResult(true);
            public Task<int> TerminateAllUserSessionsAsync(int userId) => Task.FromResult(1);
            public Task<(int ActiveCount, int TodayLogins, int TodayLogouts, int TotalSessions)> GetActivityStatsAsync() => Task.FromResult((0, 0, 0, 0));
            public Task<UserSessionModel?> GetSessionByIdAsync(int sessionId) => Task.FromResult<UserSessionModel?>(null);
            public Task<List<UserSessionModel>> GetActiveSessionsForUserAsync(int userId, int? excludeSessionId = null) => Task.FromResult(new List<UserSessionModel>());
            public Task<List<UserSessionModel>> GetActiveSessionsForEmailAsync(string email) => Task.FromResult(new List<UserSessionModel>());
            public Task<UserSessionModel?> FindActiveSessionByTokenAsync(int userId, string token) => Task.FromResult<UserSessionModel?>(null);
            public Task TouchSessionAsync(int sessionId, string clientIp) => Task.CompletedTask;
            public Task<int> GetActiveSessionsCountAsync() => Task.FromResult(0);
            public Task<bool> TerminateSessionWithAuditAsync(int sessionId, int adminUserId) => Task.FromResult(true);
            public Task<int> ForceLogoutUserWithAuditAsync(int targetUserId, int adminUserId) => Task.FromResult(1);
            public Task AddSessionAsync(UserSessionModel session) => Task.CompletedTask;

            public Task<UserSessionModel?> GetByIdAsync(int id) => Task.FromResult<UserSessionModel?>(null);
            public Task<List<UserSessionModel>> ListAllAsync() => Task.FromResult(new List<UserSessionModel>());
            public Task<List<UserSessionModel>> FindAsync(System.Linq.Expressions.Expression<Func<UserSessionModel, bool>> predicate) => Task.FromResult(new List<UserSessionModel>());
            public Task<UserSessionModel?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<UserSessionModel, bool>> predicate) => Task.FromResult<UserSessionModel?>(null);
            public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<UserSessionModel, bool>>? predicate = null) => Task.FromResult(false);
            public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<UserSessionModel, bool>>? predicate = null) => Task.FromResult(0);
            public Task AddAsync(UserSessionModel entity) => Task.CompletedTask;
            public Task AddRangeAsync(IEnumerable<UserSessionModel> entities) => Task.CompletedTask;
            public void Update(UserSessionModel entity) { }
            public void Delete(UserSessionModel entity) { }
            public void DeleteRange(IEnumerable<UserSessionModel> entities) { }
        }

        private class FakeAuditLogRepository : IAuditLogRepository
        {
            private readonly Action _onAddLog;
            public FakeAuditLogRepository(Action onAddLog) => _onAddLog = onAddLog;

            public Task<AuditLogModel> CreateAuditLogAsync(string action, string module, string performedBy, string details, string ipAddress, string status)
            {
                _onAddLog();
                return Task.FromResult(new AuditLogModel
                {
                    Action = action,
                    Module = module,
                    PerformedBy = performedBy,
                    Details = details,
                    IpAddress = ipAddress,
                    Status = status
                });
            }

            public Task AddAuditLogAsync(AuditLogModel log)
            {
                _onAddLog();
                return Task.CompletedTask;
            }

            public Task<(List<AuditLogModel> Logs, int TotalEvents, int SuccessfulLogins, int PrivilegeChanges)> GetAuditLogsOverviewAsync(string? module, string? search) => Task.FromResult((new List<AuditLogModel>(), 0, 0, 0));
            public Task<bool> SoftDeleteAuditLogAsync(int id) => Task.FromResult(true);
            public Task<List<AuditLogModel>> GetRecentAuditLogsAsync(int count) => Task.FromResult(new List<AuditLogModel>());
            public Task<List<AuditLogModel>> GetAuditLogsInDateRangeAsync(DateTime startDate, DateTime endDate) => Task.FromResult(new List<AuditLogModel>());
        }

        private class FakeJwtService : IJwtService
        {
            public string GenerateToken(UserModel user, string? roleName = null, IEnumerable<string>? permissions = null, int? sessionId = null) => "mock_jwt_token";
            public System.Security.Claims.ClaimsPrincipal? GetPrincipalFromToken(string token) => null;
            public (string? Email, string? Name, string? Picture) ReadTokenPayload(string idToken) => (null, null, null);
        }

        private class FakeOtpService : IOtpService
        {
            public string GenerateOtp(string email, int expiryMinutes = 10) => "123456";
            public bool ValidateOtp(string email, string code, out string? errorMessage) { errorMessage = null; return true; }
            public bool ConsumeOtp(string email, string code, out string? errorMessage) { errorMessage = null; return true; }
        }

        private class FakeEmailService : IEmailService
        {
            public int Sent2FaEmailsCount { get; private set; }
            public int SentPasswordResetEmailsCount { get; private set; }
            public bool ShouldThrow { get; set; }
            public Task SendEmailAsync(string toEmail, string subject, string htmlBody) => Task.CompletedTask;
            public Task SendWelcomeUserEmailAsync(string recipientEmail, string recipientName, string plainPassword) => Task.CompletedTask;
            public Task SendTwoFactorOtpEmailAsync(string toEmail, string userName, string otpCode, int expiryMinutes = 10)
            {
                if (ShouldThrow) throw new InvalidOperationException("The SMTP server requires a secure connection or the client was not authenticated.");
                Sent2FaEmailsCount++;
                return Task.CompletedTask;
            }
            public Task SendPasswordResetOtpEmailAsync(string toEmail, string userName, string otpCode, int expiryMinutes = 10)
            {
                if (ShouldThrow) throw new InvalidOperationException("The SMTP server requires a secure connection or the client was not authenticated.");
                SentPasswordResetEmailsCount++;
                return Task.CompletedTask;
            }
            public Task SendPasswordChangedNotificationAsync(string toEmail, string userName) => Task.CompletedTask;
        }
    }
}
