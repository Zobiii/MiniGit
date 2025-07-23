using MiniGit.Utils;
using Spectre.Console.Cli;

namespace MiniGit.Commands;

public sealed class LogCommand : Command<LogCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commits = CommitManager.LoadCommits()
            .OrderByDescending(c => c.Timestamp)
            .ToList();

        if (!commits.Any())
        {
            Output.Console("No commit");
            return 0;
        }

        foreach (var commit in commits)
        {
            System.Console.WriteLine($"\nCommit {commit.Id} - {commit.Timestamp:G}");
            System.Console.WriteLine($"     - Dateien: {commit.Files.Count}");
            System.Console.WriteLine($"     - Nachricht: {commit.Message}\n");
        }

        return 0;
    }
}


