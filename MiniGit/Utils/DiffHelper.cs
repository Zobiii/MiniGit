namespace MiniGit.Utils
{
    public static class DiffHelper
    {
        public static void PrintLineDiff(string[] oldLines, string[] newLines)
        {
            int max = Math.Max(oldLines.Length, newLines.Length);
            for (int i = 0; i < max; i++)
            {
                string oldLine = i < oldLines.Length ? oldLines[i] : "";
                string newLine = i < newLines.Length ? newLines[i] : "";

                if (oldLine != newLine)
                {
                    if (!string.IsNullOrEmpty(oldLine))
                        Console.WriteLine($" - {oldLine}");

                    if (!string.IsNullOrEmpty(newLine))
                        Console.WriteLine($" + {newLine}");
                }
            }
        }
    }
}
