using Spectre.Console.Cli;
using MiniGit.Core;
using Spectre.Console;
using MiniGit.Utils;
using System.Runtime.CompilerServices;

namespace MiniGit.Commands;

public sealed class SummaryCommand : Command<SummaryCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commits = CommitManager.LoadCommits();

        if (commits.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No commits found[/]");
            return 1;
        }

        var last = commits.Last();
        var ignore = FileHelper.LoadIgnorePatterns();

        var files = Directory
            .GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)
            .Where(f => !FileHelper.ShouldIgnore(f, ignore))
            .Select(f => Path.GetRelativePath(Directory.GetCurrentDirectory(), f))
            .ToList();

        var hashes = files.ToDictionary(
            f => f,
            f => FileHasher.ComputeHash(Path.Combine(Directory.GetCurrentDirectory(), f))
        );

        var allKeys = new HashSet<string>(last.Files.Keys.Concat(hashes.Keys));

        var table = new Table().Border(TableBorder.Rounded).Title("\nLast Commits");
        table.AddColumn("ID");
        table.AddColumn("Timestamp");
        table.AddColumn("Message");

        var lastCommits = commits
            .OrderByDescending(c => c.Timestamp)
            .Take(3)
            .ToList();

        foreach (var c in lastCommits)
            table.AddRow(c.Id, c.Timestamp.ToString("g"), c.Message);

        AnsiConsole.Write(table);
        System.Console.WriteLine();

        AnsiConsole.MarkupLine("[bold underline]📁 File Status:[/]");
        foreach (var key in allKeys.OrderBy(k => k))
        {
            var inCommit = last.Files.ContainsKey(key);
            var inCurrent = hashes.ContainsKey(key);

            if (inCommit && inCurrent)
            {
                if (last.Files[key] == hashes[key])
                    AnsiConsole.MarkupLine($"[green]✅ {key} — unchanged[/]");
                else
                    AnsiConsole.MarkupLine($"[red]❌ {key} — modified[/]");
            }
            else if (inCommit && !inCurrent)
            {
                AnsiConsole.MarkupLine($"[yellow]⚠️ {key} — missing[/]");
            }
            else if (!inCommit && inCurrent)
            {
                AnsiConsole.MarkupLine($"[blue]➕ {key} — untracked[/]");
            }
        }
        return 0;
    }
}