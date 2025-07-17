using Spectre.Console.Cli;
using MiniGit.Core;

namespace MiniGit.Commands;

public sealed class StatusCommand : Command<StatusCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var statusInfo = CommandHandler.GetStatusInfo();

        if (!statusInfo.HasChanges)
        {
            Console.WriteLine("✅ Keine Änderungen seit letztem Commit.");
            return 0;
        }

        if (statusInfo.NewFiles.Any())
        {
            Console.WriteLine("\n🆕 Neue Dateien:");
            foreach (var f in statusInfo.NewFiles) Console.WriteLine("  + " + f);
        }

        if (statusInfo.ChangedFiles.Any())
        {
            Console.WriteLine("\n✏️  Geänderte Dateien:");
            foreach (var f in statusInfo.ChangedFiles) Console.WriteLine("  ~ " + f);
        }

        if (statusInfo.DeletedFiles.Any())
        {
            Console.WriteLine("\n❌ Gelöschte Dateien:");
            foreach (var f in statusInfo.DeletedFiles) Console.WriteLine("  - " + f);
        }
        return 0;
    }
}


