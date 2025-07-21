using System.Security.Cryptography;
using MiniGit.Utils;


public static class FileHasher
{
    public static string ComputeHash(string filePath)
    {
        Logger.INFO("Starting hash creation");
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        Logger.DEBUG($"Hash for file: {filePath} generated");
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

