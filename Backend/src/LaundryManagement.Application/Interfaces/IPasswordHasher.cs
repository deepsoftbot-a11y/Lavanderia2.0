namespace LaundryManagement.Application.Interfaces;

/// <summary>
/// Service interface for password hashing and verification
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    string HashPassword(string plainText);

    /// <summary>
    /// Verifies a plain text password against a hash
    /// </summary>
    bool VerifyPassword(string plainText, string hash);
}
