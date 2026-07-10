namespace MiniDbProject.Exceptions;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public bool IsUserFriendly { get; }

    public AppException(string message, string errorCode = "GENERAL_ERROR", bool isUserFriendly = false)
        : base(message)
    {
        ErrorCode = errorCode;
        IsUserFriendly = isUserFriendly;
    }

    public AppException(string message, Exception innerException, string errorCode = "GENERAL_ERROR", bool isUserFriendly = false)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        IsUserFriendly = isUserFriendly;
    }
}
