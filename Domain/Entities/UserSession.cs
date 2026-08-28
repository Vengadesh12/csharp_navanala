using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Business object representing user login and logout audit sessions with state transitions.
    /// </summary>
    [Table("user_sessions")]
    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = "127.0.0.1";
        public string? UserAgent { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public DateTime? LogoutTime { get; set; }
        public string? SessionToken { get; set; }
        public bool IsActive { get; set; } = true;
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create and start a new User Session.
        /// </summary>
        public static UserSession Start(
            int userId,
            string email,
            string userName,
            string ipAddress,
            string? userAgent,
            string? sessionToken)
        {
            return new UserSession
            {
                UserId = userId,
                Email = email.Trim(),
                UserName = userName.Trim(),
                IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim(),
                UserAgent = userAgent,
                SessionToken = sessionToken,
                LoginTime = DateTime.UtcNow,
                LogoutTime = null,
                IsActive = true,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Ends and terminates the user session.
        /// </summary>
        public void EndSession(DateTime? logoutTime = null)
        {
            IsActive = false;
            LogoutTime = logoutTime ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the session record.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
        }

        #endregion
    }
}
