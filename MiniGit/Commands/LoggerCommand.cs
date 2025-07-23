using System.Runtime.InteropServices;
using MiniGit.Utils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MiniGit.Commands;

public class LoggerCommand : Command<LoggerCommand.Settings>
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
            Output.Console($"[red]Error[/]: Use minigit logger true/false");
            return false;
        }

        Logger.SetEnabled(enable);
        Output.Console($"Logger {(enable ? "enabled" : "disabled")}");
        return 0;
    }
}