using LaundryManagement.Application.Interfaces;
using BCrypt.Net;

namespace LaundryManagement.Infrastructure.Services;

/// <summary>
/// Password hashing service using BCrypt
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // BCrypt work factor (2^12 iterations)

    public string HashPassword(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentException("Password cannot be empty", nameof(plainText));
        }

        return BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);
    }

    public bool VerifyPassword(string plainText, string hash)
    {
        if (string.IsNullOrWhiteSpace(plainText) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hash);
        }
        catch
        {
            // If verification fails for any reason (malformed hash, etc), return false
            return false;
        }
    }
}
