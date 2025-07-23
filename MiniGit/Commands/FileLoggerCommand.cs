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
            _ => HandleInvalidState()
        };

        bool HandleInvalidState()
        {
            Output.Console($"[red]Error[/]: Use minigit flogger true/false");
            return false;
        }

        FileLogger.SetEnabled(enable);
        Output.Console($"FileLogger {(enable ? "enabled" : "disabled")}");
        return 0;
    }
}