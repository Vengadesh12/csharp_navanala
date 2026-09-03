using System;
using System.Collections.Generic;

namespace MyBackend.Application.DTO;

/// <summary>
/// Standard API response wrapper containing a success flag, message, and optional payload.
/// </summary>
/// <typeparam name="T">The type of payload data.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Informational message describing the result of the request.
    /// </summary>
    /// <example>Operation completed successfully.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The response payload data.
    /// </summary>
    public T? Data { get; set; }
}

/// <summary>
/// Generic message response for operations that do not return a data entity.
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Informational message or status description.
    /// </summary>
    /// <example>Request processed successfully.</example>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Error response details returned on client or server errors (400, 401, 403, 404, 500).
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Indicates failure of the request.
    /// </summary>
    /// <example>false</example>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Error message explaining why the request failed.
    /// </summary>
    /// <example>Invalid request parameters or unauthorized access.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of validation or field-level errors.
    /// </summary>
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Response returned upon successful soft deletion or restoration of a resource.
/// </summary>
public class DeleteResponse
{
    /// <summary>
    /// Indicates whether the deletion or status update succeeded.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Status description message.
    /// </summary>
    /// <example>Resource deleted successfully!</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the affected resource.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// The updated deleted flag state (0 = deleted/inactive, 1 = active).
    /// </summary>
    /// <example>0</example>
    public int DeletedFlag { get; set; }
}
