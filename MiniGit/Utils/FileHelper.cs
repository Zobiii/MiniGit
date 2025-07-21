namespace MiniGit.Utils;

public static class FileHelper
{
    public static List<string> LoadIgnorePatterns(string ignoreFile = ".minigitignore")
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), ignoreFile);
        Logger.INFO($"Loading ignore patterns from '{ignoreFile}'");

        if (!File.Exists(fullPath))
        {
            Logger.WARN($"{ignoreFile} file not found - tracking all files");
            return new();
        }

        var patterns = File.ReadAllLines(fullPath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .ToList();

        Logger.DEBUG($"Ignore patterns loaded: {string.Join(", ", patterns)}");

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
                {
                    FileLogger.DEBUG($"File '{filePath}' is ignored due to directory pattern '{pattern}'");
                    return true;
                }
            }
            else if (p.StartsWith("*.")) // Dateiendung
            {
                if (normalizedPath.EndsWith(p[1..]))
                {
                    FileLogger.DEBUG($"File '{filePath}' is ignored due to extension pattern '{pattern}'");
                    return true;
                }
            }
            else if (normalizedPath.EndsWith("/" + p) || normalizedPath.EndsWith(p)) // Teilpfad oder Dateiname
            {
                FileLogger.DEBUG($"File '{filePath}' is ignored due to keyword match '{pattern}'");
                return true;
            }
        }
        Logger.INFO($"File {filePath} is not ignored");
        return false;
    }
}
