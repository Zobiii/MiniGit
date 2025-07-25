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
        Logger.DEBUG("Command log executed");

        Logger.DEBUG("Getting all commits");

        var commits = CommitManager.LoadCommits()
            .OrderByDescending(c => c.Timestamp)
            .ToList();

        if (!commits.Any())
        {
            Output.Console("No commits found");
            return 0;
        }

        Output.Break();

        foreach (var commit in commits)
        {
            Output.Console($"Commit {commit.Id} - {commit.Timestamp:G}");
            Output.Console($"     - Dateien: {commit.Files.Count}");
            Output.Console($"     - Nachricht: {commit.Message}");
            Output.Break();
        }

        return 0;
    }
}


