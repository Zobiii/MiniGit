using MiniGit.Utils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MiniGit.Commands;

public class LogTreeCommand : Command<LogTreeCommand.Settings>
{
    public class Settings : CommandSettings { }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commits = CommitManager.LoadCommits();

        if (commits.Count == 0)
        {
            return 0;
        }

        var tree = new Tree("Commit History");

        for (int i = 0; i < commits.Count; i++)
        {
            var c = commits[i];
            var node = $"{c.Id} - {c.Timestamp:yyyy-MM-dd HH:mm:ss} - {c.Message}";
            tree.AddNode(node);
        }

        AnsiConsole.Write(tree);
        return 0;
    }
}