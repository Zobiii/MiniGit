namespace MiniGit.Utils;

using System.Diagnostics;
using System.Reflection.Emit;
using Spectre.Console;

public static class Logger
{
    private static bool _enabled = LoadStatus();
    private static readonly string _logPath = Path.Combine(".minigit", "logs.txt");

    private static bool LoadStatus()
    {
        string path = Path.Combine(".minigit", "logger.config");
        return File.Exists(path) && File.ReadAllText(path).Trim() == "true";
    }

    private static void Log(string level, string message)
    {
        if (!_enabled) return;

        var caller = new StackTrace().GetFrame(2)?.GetMethod()?.DeclaringType?.Name ?? "Unknown";

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string fileLog = $"{timestamp} - {caller}.cs - {level.ToUpper()} - {message}";

        Directory.CreateDirectory(".minigit");
        File.AppendAllText(_logPath, fileLog + Environment.NewLine);

        var tag = level.ToUpper() switch
        {
            "INFO" => "[blue][[INFO]][/]",
            "WARN" => "[yellow][[WARN]][/]",
            "ERROR" => "[red][[ERROR]][/]",
            "DEBUG" => "[darkgoldenrod][[DEBUG]][/]",
            _ => "[white][[LOG]][/]"
        };

        AnsiConsole.MarkupLine($"{tag} - {caller}.cs - {message}");
    }

    public static void INFO(string msg) => Log("INFO", msg);
    public static void WARN(string msg) => Log("WARN", msg);
    public static void ERROR(string msg) => Log("ERROR", msg);
    public static void DEBUG(string msg) => Log("DEBUG", msg);

    public static void SetEnabled(bool enable)
    {
        Directory.CreateDirectory(".minigit");
        File.WriteAllText(Path.Combine(".minigit", "logger.config"), enable.ToString().ToLower());
        _enabled = enable;
    }

    public static bool IsEnable() => _enabled;
}