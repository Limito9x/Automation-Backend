using FluentResults;

namespace Automation.SharedKernel.Errors;

/// <summary>
/// Lỗi 401 — dùng ErrorCode để phân biệt loại lỗi:
/// UNAUTHORIZED, TOKEN_EXPIRED, TOKEN_REVOKED, TOKEN_MISSING
/// </summary>
public class UnauthorizedError(string message, string errorCode = "UNAUTHORIZED") : Error(message)
{
    public string ErrorCode { get; } = errorCode;
}

