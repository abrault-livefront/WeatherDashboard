namespace WeatherDashboard.Application.Common.Utilities;

using System.Security.Cryptography;
using System.Text;

/// <summary>
///     Provides hashing utilities.
/// </summary>
public static class HashUtility
{
    /// <summary>
    ///     Computes the SHA-256 hash of the specified UTF-8 string and returns the hash as an uppercase hexadecimal string.
    /// </summary>
    /// <param name="input">The non-null, non-whitespace string to hash.</param>
    /// <returns>The SHA-256 hash represented as an uppercase hexadecimal string.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="input" /> is null, empty, or consists only of
    ///     whitespace.
    /// </exception>
    public static string HashString(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
