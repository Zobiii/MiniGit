using Spectre.Console.Cli;
using MiniGit.Utils;
using Spectre.Console;
using MiniGit.Core;

namespace MiniGit.Commands;

public sealed class AmendCommand : Command<AmendCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[message]")]
        public string[] Message { get; set; } = [];
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commits = CommitManager.LoadCommits();

        if (commits == null)
        {
            AnsiConsole.MarkupLine("[red]No commit to amend[/]");
            return 1;
        }

        var last = commits.Last();

        var ignore = FileHelper.LoadIgnorePatterns();

        var files = Directory
            .GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)
            .Where(f => !FileHelper.ShouldIgnore(f, ignore))
            .ToList();

        var hashes = files.ToDictionary(
            f => Path.GetRelativePath(Directory.GetCurrentDirectory(), f),
            FileHasher.ComputeHash
        );

        last.Timestamp = DateTime.Now;
        last.Message = settings.Message.Length > 0 ? string.Join(" ", settings.Message) : last.Message;
        last.Files = hashes;

        bool success = CommitManager.SaveCommits(commits);
        if (!success)
        {
            AnsiConsole.MarkupLine("[red]Fehler: Commit konnte nicht gespeichert werden (Repository gesperrt?)[/]");
            return 1;
        }
        CommandHandler.ReplaceSnapshot(last.Id, files, hashes);

        AnsiConsole.MarkupLine($"[green]Commit [bold]{last.Id}[/] amended.[/]");
        return 0;
    }
}