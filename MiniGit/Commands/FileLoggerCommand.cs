using System.Runtime.InteropServices;
using MiniGit.Utils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MiniGit.Commands;

public class FileLoggerCommand : Command<FileLoggerCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<state>")]
        public string State { get; set; } = "";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        bool enable = settings.State.Trim().ToLower() switch
        {
            "true" => true,
            "false" => false,
            _ => throw new Exception("Use: minigit logger true|false")
        };

        FileLogger.SetEnabled(enable);
        AnsiConsole.MarkupLine($"[green]FileLogger {(enable ? "enabled" : "disabled")}[/]");
        return 0;
    }
}