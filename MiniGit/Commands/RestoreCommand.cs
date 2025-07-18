using Spectre.Console.Cli;
using MiniGit.Core;
using Spectre.Console;

namespace MiniGit.Commands;

public sealed class RestoreCommand : Command<RestoreCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<file>")]
        public string? File { get; set; } = default;

        [CommandArgument(1, "[commitID]")]
        public string? CommitID { get; set; }

        public override ValidationResult Validate()
        {
            return string.IsNullOrWhiteSpace(File)
                ? ValidationResult.Error("File name must be provided.")
                : ValidationResult.Success();
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commits = CommitManager.LoadCommits();

        Commit? commit = null;

        if (!string.IsNullOrWhiteSpace(settings.CommitID))
            commit = CommitManager.GetCommitById(settings.CommitID);
        else if (commits.Count > 0)
            commit = commits.Last();
        else
        {
            AnsiConsole.MarkupLine("[red]No commits found.[/]");
            return 1;
        }

        var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), settings.File!);

        if (commit == null || !commit.Files.ContainsKey(relativePath))
        {
            AnsiConsole.MarkupLine($"[red]The file '{relativePath}' does not exist in commit {(commit != null ? commit.Id : "N/A")}.[/]");
            return 1;
        }

        var snapshotPath = Path.Combine(".minigit", "snapshots", commit.Id, relativePath);

        if (!File.Exists(snapshotPath))
        {
            AnsiConsole.MarkupLine($"[red]Snapshot not found for file:[/] {snapshotPath}");
            return 1;
        }

        var destDir = Path.GetDirectoryName(settings.File);

        if (!string.IsNullOrWhiteSpace(destDir))
            Directory.CreateDirectory(destDir);

        if (settings.File == null)
        {
            AnsiConsole.MarkupLine("[red]Destination file path is null.[/]");
            return 1;
        }

        File.Copy(snapshotPath, settings.File, overwrite: true);

        AnsiConsole.MarkupLine($"[green]Restored[/] '{relativePath}' from commit [yellow]{commit.Id}[/]");

        return 0;
    }
}