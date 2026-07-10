using System.Security.Cryptography;

namespace MiniDbProject.Helpers;

public static class PasswordHelper
{
    public const int SaltSize = 16;
    public const int HashSize = 32;
    public const int Iterations = 100000;

    public static string GenerateSalt()
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(salt);
    }

    public static string HashPassword(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        string computedHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(hash)
        );
    }

    public static bool IsStrongPassword(string password)
    {
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
        return true;
    }

    public static string GetPasswordStrengthMessage(string password)
    {
        if (password.Length < 8) return "Too short (min 8 characters)";
        int score = 0;
        if (password.Length >= 12) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score++;

        return score switch
        {
            <= 2 => "Weak",
            3 => "Medium",
            4 => "Strong",
            >= 5 => "Very Strong"
        };
    }
}
