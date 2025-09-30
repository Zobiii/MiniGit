using Spectre.Console.Cli;
using Spectre.Console;
using System.IO.Compression;

namespace MiniGit.Commands;

public sealed class ExportCommand : Command<ExportCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<commitId>")]
        public string CommitId { get; set; } = "";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commitId = settings.CommitId.Trim();
        var commit = CommitManager.GetCommitById(commitId);

        if (commit == null)
        {
            AnsiConsole.MarkupLine("[red]Commit not found[/]");
            return 1;
        }

        var snapshotPath = Path.Combine(".minigit", "snapshots", commit.Id);

        if (!Directory.Exists(snapshotPath))
        {
            AnsiConsole.MarkupLine($"[red]✘ Snapshot folder for commit [bold]{commit.Id}[/] not found.[/]");
            return 1;
        }

        var zipName = $"export_{commit.Id}.zip";

        try
        {
            if (File.Exists(zipName))
                File.Delete(zipName);

            ZipFile.CreateFromDirectory(snapshotPath, zipName);

            AnsiConsole.MarkupLine($"[green]Export successful:[/] [underline]{zipName}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 2;
        }
    }
}