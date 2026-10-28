using System.Security.Cryptography;
using System.Text;

namespace Edutor.Core.Modules.Nodes;

/// <summary>Worker access token-lərinin yaradılması və hash-lənməsi.</summary>
public static class NodeTokens
{
    public static string Create()
        => Base64Url(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string token)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    /// <summary>Sabit vaxtlı müqayisə — token sızmasının qarşısını alır.</summary>
    public static bool Matches(string provided, string storedHash)
        => CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Hash(provided)),
            Encoding.UTF8.GetBytes(storedHash));

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
