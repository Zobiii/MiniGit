using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGit.Utils;

public static class FileHelper
{
    public static List<string> LoadIgnorePatterns(string ignoreFile = ".minigitignore")
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), ignoreFile);

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"⚠️  Datei '{ignoreFile}' nicht gefunden im aktuellen Verzeichnis: {Directory.GetCurrentDirectory()}");
            return new();
        }

        var patterns = File.ReadAllLines(fullPath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .ToList();

        return patterns;
    }

    public static bool ShouldIgnore(string filePath, List<string> ignorePatterns)
    {
        string normalizedPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .ToLower();

        foreach (var pattern in ignorePatterns)
        {
            string p = pattern.Replace("\\", "/").Trim().ToLower();

            if (p.EndsWith("/")) // Ordner
            {
                if (normalizedPath.Contains(p.TrimEnd('/')))
                    return true;
            }
            else if (p.StartsWith("*.")) // Dateiendung
            {
                if (normalizedPath.EndsWith(p[1..]))
                    return true;
            }
            else if (normalizedPath.EndsWith("/" + p) || normalizedPath.EndsWith(p)) // Teilpfad oder Dateiname
            {
                return true;
            }
        }

        return false;
    }
}
