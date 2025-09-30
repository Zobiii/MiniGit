namespace MiniGit.Utils
{
    public static class DiffHelper
    {
        public static void PrintLineDiff(string[] oldLines, string[] newLines)
        {
            Logger.INFO("Starting line-by-line diff");

            int max = Math.Max(oldLines.Length, newLines.Length);
            bool anyDiff = false;

            for (int i = 0; i < max; i++)
            {
                anyDiff = true;
                string oldLine = i < oldLines.Length ? oldLines[i] : "";
                string newLine = i < newLines.Length ? newLines[i] : "";

                if (oldLine != newLine)
                {
                    if (!string.IsNullOrEmpty(oldLine))
                        Console.WriteLine($" - {oldLine}");

                    if (!string.IsNullOrEmpty(newLine))
                        Console.WriteLine($" + {newLine}");

                    Logger.DEBUG($"Line {i + 1} differ");
                }
            }
            if (!anyDiff)
                Logger.INFO("No differences found");
        }
    }
}
