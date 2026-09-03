using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
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

        [HttpGet("maintenance-status")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MaintenanceStatusResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMaintenanceStatus()
        {
            var status = await _authService.GetMaintenanceStatusAsync();
            return Ok(status);
        }

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

        [HttpPost("google-login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.IdToken))
            {
                return BadRequest(new ErrorResponse { Message = "Google email address or ID token is required." });
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers.UserAgent.ToString();

            try
            {
                var response = await _authService.GoogleLoginAsync(request, ipAddress, userAgent);
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = ex.Message });
            }
        }

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

        [HttpGet("sessions/all")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserSessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllRecentSessions([FromQuery] int limit = 100)
        {
            var sessions = await _authService.GetAllRecentSessionsAsync(limit);
            return Ok(sessions);
        }

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

        [HttpPost("validate-password")]
        [HttpPost("evaluate-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EvaluatePasswordResponse), StatusCodes.Status200OK)]
        public IActionResult ValidatePassword([FromBody] EvaluatePasswordRequest request)
        {
            var response = _authService.EvaluatePassword(request);
            return Ok(response);
        }

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
