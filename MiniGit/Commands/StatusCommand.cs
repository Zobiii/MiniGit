using Spectre.Console.Cli;
using MiniGit.Core;
using MiniGit.Utils;

namespace MiniGit.Commands;

public sealed class StatusCommand : Command<StatusCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        Logger.DEBUG("Command status executed");

        var statusInfo = CommandHandler.GetStatusInfo();

        Logger.DEBUG($"Information recieved: {statusInfo.NewFiles.Count} new files, {statusInfo.ChangedFiles.Count} changed files and {statusInfo.DeletedFiles.Count} deleted files");

        if (!statusInfo.HasChanges)
        {
            Output.Break();
            Output.Console("Keine Änderungen seit letztem Commit.");
            Output.Break();
            return 0;
        }

        if (statusInfo.NewFiles.Any())
        {
            Output.Break();
            Output.Console("Neue Dateien:");
            foreach (var f in statusInfo.NewFiles) Output.Console("  + " + f);
        }

        if (statusInfo.ChangedFiles.Any())
        {
            Output.Break();
            Output.Console("Geänderte Dateien:");
            foreach (var f in statusInfo.ChangedFiles) Output.Console("  ~ " + f);
        }

        if (statusInfo.DeletedFiles.Any())
        {
            Output.Break();
            Output.Console("Gelöschte Dateien:");
            foreach (var f in statusInfo.DeletedFiles) Output.Console("  - " + f);
        }

        Output.Break();
        return 0;
    }
}


