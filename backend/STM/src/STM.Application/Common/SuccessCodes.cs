namespace STM.Application.Common;

public static class SuccessCodes
{
    // User Registration
    public const string UserRegistered = "USER_REGISTERED";
    public const string OtpSent = "OTP_SENT";

    // OTP
    public const string OtpVerified = "OTP_VERIFIED";

    // Login
    public const string LoginSuccessful = "LOGIN_SUCCESSFUL";
    public const string TokenRefreshed = "TOKEN_REFRESHED";

    // Tasks
    public const string TaskCreated = "TASK_CREATED";
    public const string TaskUpdated = "TASK_UPDATED";
    public const string TaskDeleted = "TASK_DELETED";

    // System
    public const string OperationSuccessful = "OPERATION_SUCCESSFUL";
}