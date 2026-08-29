using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Authentication, 2FA validation, user session tracking (login/logout with IP address and timestamps), and password evaluation endpoints.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Tags("Authentication")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticate user credentials, record login session with client IP address &amp; timestamp in database, and generate JWT access token.
        /// </summary>
        /// <param name="request">User login credentials containing email and password.</param>
        /// <response code="200">Authentication successful (or 2FA required); returns user profile, permissions, dynamic menus, and JWT token.</response>
        /// <response code="400">Email or password was not provided.</response>
        /// <response code="401">Invalid credentials or account is deactivated.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse { Message = "Email and password are required." });
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers.UserAgent.ToString();

            try
            {
                var response = await _authService.LoginAsync(request, ipAddress, userAgent);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Verify Two-Factor Authentication (2FA) OTP code, record active session with IP address in database, and generate JWT token upon validation.
        /// </summary>
        /// <param name="request">Payload containing user email and 6-digit 2FA OTP.</param>
        /// <response code="200">2FA OTP verified; returns authenticated session data and JWT token.</response>
        /// <response code="400">Invalid or expired 2FA code.</response>
        /// <response code="404">User account not found.</response>
        [HttpPost("login-2fa-verify")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyTwoFactorLogin([FromBody] Verify2FaLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new ErrorResponse { Message = "Email and 6-digit OTP code are required." });
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers.UserAgent.ToString();

            try
            {
                var response = await _authService.Verify2FaLoginAsync(request, ipAddress, userAgent);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Record user logout time, date, and IP address in the database and deactivate active session.
        /// </summary>
        /// <response code="200">Logout session recorded and session terminated successfully.</response>
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
        {
            int userId = 0;
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId))
            {
                userId = parsedId;
            }
            else if (request?.UserId.HasValue == true)
            {
                userId = request.UserId.Value;
            }

            var ipAddress = GetClientIpAddress();
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();

            var response = await _authService.LogoutAsync(userId, ipAddress, token, request?.Email);
            return Ok(response);
        }

        /// <summary>
        /// Retrieve the session history (login times, logout times, dates, and IP addresses) for the current user.
        /// </summary>
        /// <response code="200">List of user sessions returned successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet("sessions")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserSessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserSessions([FromQuery] int limit = 50)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "A valid authenticated user session is required." });
            }

            var sessions = await _authService.GetUserSessionsAsync(userId, limit);
            return Ok(sessions);
        }

        /// <summary>
        /// Retrieve all recent workspace user sessions across the system (Requires Super Admin or permissions.manage).
        /// </summary>
        /// <response code="200">List of workspace user sessions returned successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet("sessions/all")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserSessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllRecentSessions([FromQuery] int limit = 100)
        {
            var sessions = await _authService.GetAllRecentSessionsAsync(limit);
            return Ok(sessions);
        }

        /// <summary>
        /// Resend 2FA login verification code to the user's registered email.
        /// </summary>
        /// <param name="request">Payload containing registered user email.</param>
        /// <response code="200">New 2FA code dispatched via email.</response>
        /// <response code="400">Email payload is invalid.</response>
        /// <response code="404">User account not found.</response>
        [HttpPost("resend-2fa-otp")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResendTwoFactorOtp([FromBody] Resend2FaOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new ErrorResponse { Message = "Email address is required." });
            }

            try
            {
                var response = await _authService.Resend2FaOtpAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Message = $"Unable to dispatch 2FA email: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Initiate password reset by dispatching a 6-digit verification OTP to the user's registered email.
        /// </summary>
        /// <param name="request">Payload containing the registered email address.</param>
        /// <response code="200">OTP generated and dispatched via email successfully.</response>
        /// <response code="400">Invalid email payload.</response>
        /// <response code="404">No active user account found with this email.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new ErrorResponse { Message = "Email address is required." });
            }

            try
            {
                var response = await _authService.ForgotPasswordAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Message = $"Unable to dispatch OTP email: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Validate the OTP entered by the user before password update.
        /// </summary>
        /// <param name="request">Email and 6-digit OTP code.</param>
        /// <response code="200">OTP is valid and ready for password reset.</response>
        /// <response code="400">Invalid or expired OTP code.</response>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var response = _authService.VerifyOtp(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Reset user password using verified OTP with strong password validation.
        /// </summary>
        /// <param name="request">Email, OTP, and new password payload.</param>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">OTP invalid, passwords mismatch, or password does not satisfy strong security requirements.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var response = await _authService.ResetPasswordAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Evaluate password strength and complexity rules in real-time.
        /// </summary>
        /// <param name="request">Payload containing the candidate password string.</param>
        /// <response code="200">Password evaluation completed with criteria statuses and strength score.</response>
        [HttpPost("validate-password")]
        [HttpPost("evaluate-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EvaluatePasswordResponse), StatusCodes.Status200OK)]
        public IActionResult ValidatePassword([FromBody] EvaluatePasswordRequest request)
        {
            var response = _authService.EvaluatePassword(request);
            return Ok(response);
        }

        /// <summary>
        /// Retrieve the permission keys for the currently authenticated user.
        /// </summary>
        /// <response code="200">Returns list of granted permission keys.</response>
        /// <response code="401">Unauthorized: Valid JWT Bearer token missing or user deactivated.</response>
        [HttpGet("permissions")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserPermissionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "A valid user is required." });
            }

            try
            {
                var response = await _authService.GetUserPermissionsAsync(userId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ErrorResponse { Message = ex.Message });
            }
        }

        private string GetClientIpAddress()
        {
            // 1. Check X-Forwarded-For header (reverse proxy / load balancer)
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader) && !string.IsNullOrWhiteSpace(forwardedHeader))
            {
                var ip = forwardedHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    return ip;
                }
            }

            // 2. Check X-Real-IP header
            if (Request.Headers.TryGetValue("X-Real-IP", out var realIpHeader) && !string.IsNullOrWhiteSpace(realIpHeader))
            {
                var ip = realIpHeader.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    return ip;
                }
            }

            // 3. Fallback to HttpContext RemoteIpAddress
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                // Handle IPv6 loopback (::1 -> 127.0.0.1)
                if (remoteIp == "::1")
                {
                    return "127.0.0.1";
                }
                return remoteIp;
            }

            return "127.0.0.1";
        }
    }
}
