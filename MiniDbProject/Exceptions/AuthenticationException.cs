namespace MiniDbProject.Exceptions;

public class AuthenticationException : AppException
{
    public AuthenticationException(string message, string errorCode = "AUTH_ERROR")
        : base(message, errorCode, isUserFriendly: true)
    {
    }
}

public class ValidationException : AppException
{
    public ValidationException(string message, string errorCode = "VALIDATION_ERROR")
        : base(message, errorCode, isUserFriendly: true)
    {
    }
}

public class DatabaseException : AppException
{
    public DatabaseException(string message, string errorCode = "DB_ERROR")
        : base(message, errorCode, isUserFriendly: true)
    {
    }

    public DatabaseException(string message, Exception inner, string errorCode = "DB_ERROR")
        : base(message, inner, errorCode, isUserFriendly: true)
    {
    }
}

public class ConfigurationException : AppException
{
    public ConfigurationException(string message, string errorCode = "CONFIG_ERROR")
        : base(message, errorCode, isUserFriendly: true)
    {
    }
}
