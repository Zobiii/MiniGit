namespace MiniGit.Utils
{
    public static class PathValidator
    {
        public static string? ValidateAndSanitizePath(string inputPath, string? allowedBasePath = null)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Logger.WARN("Path validation failed: null or empty path");
                return null;
            }

            try
            {
                allowedBasePath ??= Directory.GetCurrentDirectory();

                string fullInputPath = Path.GetFullPath(inputPath, allowedBasePath);
                string fullBasePath = Path.GetFullPath(allowedBasePath);

                if (!fullInputPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.ERROR($"Path traversal attack detected: '{inputPath}' resolves to '{fullInputPath}' which is outside '{fullBasePath}'");
                    return null;
                }

                if (ContainsSuspiciousPatterns(inputPath))
                {
                    Logger.ERROR($"Suspicious path pattern detected: '{inputPath}'");
                    return null;
                }

                Logger.DEBUG($"Path validation successful: '{inputPath}' -> '{fullInputPath}'");
                return fullInputPath;
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Path validation error for '{inputPath}': {ex.Message}");
                return null;
            }
        }

        public static bool IsValidRepositoryPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            if (relativePath.Contains("..") ||
                relativePath.StartsWith("/") ||
                relativePath.StartsWith("\\") ||
                Path.IsPathRooted(relativePath))
            {
                Logger.WARN($"Invalid repository path: '{relativePath}'");
                return false;
            }

            return true;
        }

        private static bool ContainsSuspiciousPatterns(string path)
        {
            string[] suspiciousPatterns = {
                "..",
                "~",
                "$",
                "%",
                ":",
                "*",
                "?",
                "<",
                ">",
                "|"
            };

            string normalizedPath = path.ToLower();

            foreach (var pattern in suspiciousPatterns)
            {
                if (normalizedPath.Contains(pattern))
                {
                    return true;
                }
            }

            return false;
        }

        public static string? CreateSafeFilePath(string fileName)
        {
            if (!IsValidRepositoryPath(fileName))
                return null;

            return ValidateAndSanitizePath(fileName);
        }
    }
}