using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

public static class FileHasher
{
    public static string ComputeHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}