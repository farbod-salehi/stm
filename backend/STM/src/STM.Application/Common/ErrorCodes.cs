namespace STM.Application.Common;

public static class ErrorCodes
{
    // User Registration
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
    public const string WeakPassword = "WEAK_PASSWORD";

    // OTP
    public const string OtpInvalid = "OTP_INVALID";
    public const string OtpExpired = "OTP_EXPIRED";
    public const string OtpNotFound = "OTP_NOT_FOUND";
    public const string OtpTooManyRequests = "OTP_TOO_MANY_REQUESTS";

    // Login
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string IncorrectPassword = "INCORRECT_PASSWORD";
    public const string UserNotVerified = "USER_NOT_VERIFIED";

    // JWT / Auth
    public const string InvalidToken = "INVALID_TOKEN";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string RefreshTokenInvalid = "REFRESH_TOKEN_INVALID";
    public const string RefreshTokenExpired = "REFRESH_TOKEN_EXPIRED";

    // Tasks
    public const string TaskNotFound = "TASK_NOT_FOUND";
    public const string TaskAccessDenied = "TASK_ACCESS_DENIED";

    // System
    public const string UnknownError = "UNKNOWN_ERROR";
}