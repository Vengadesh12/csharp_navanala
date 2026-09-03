using System;
using System.Collections.Generic;

namespace MyBackend.Application.Common.DTO;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }
}

public class MessageResponse
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public List<string>? Errors { get; set; }
}

public class DeleteResponse
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = string.Empty;

    public int Id { get; set; }

    public int DeletedFlag { get; set; }
}
