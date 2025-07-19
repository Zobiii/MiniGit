using Spectre.Console.Cli;
using MiniGit.Core;
using System.Runtime.CompilerServices;
using Spectre.Console;
using MiniGit.Utils;

namespace MiniGit.Commands;

public sealed class VerifyCommand : Command<VerifyCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commits = CommitManager.LoadCommits();

        if (commits.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No commits found.[/]");
            return 1;
        }

        var last = commits.Last();
        var ignorePatterns = FileHelper.LoadIgnorePatterns();

        var currentFiles = Directory
            .GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)
            .Where(f => !FileHelper.ShouldIgnore(f, ignorePatterns))
            .Select(f => Path.GetRelativePath(Directory.GetCurrentDirectory(), f))
            .ToList();

        var currentHashes = currentFiles.ToDictionary(
            f => f,
            f => FileHasher.ComputeHash(Path.Combine(Directory.GetCurrentDirectory(), f))
        );

        var allKeys = new HashSet<string>(last.Files.Keys.Concat(currentHashes.Keys));

        foreach (var key in allKeys.OrderBy(x => x))
        {
            var inCommit = last.Files.ContainsKey(key);
            var inCurrent = currentHashes.ContainsKey(key);

            if (inCommit && inCurrent)
            {
                if (last.Files[key] == currentHashes[key])
                    AnsiConsole.MarkupLine($"[green]{key} - unchanged[/]");
                else
                    AnsiConsole.MarkupLine($"[red]{key} - modified[/]");
            }
            else if (inCommit && !inCurrent)
            {
                AnsiConsole.MarkupLine($"[yellow]{key} - missing from working dir[/]");
            }
            else if (!inCommit && inCurrent)
            {
                AnsiConsole.MarkupLine($"[blue]{key} - untracked[/]");
            }
        }
        return 0;
    }
}