using Spectre.Console.Cli;

namespace MiniGit.Commands;

public sealed class CheckoutCommand : Command<CheckoutCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<commitId>")]
        public string commitId { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var commit = CommitManager.GetCommitById(settings.commitId);
        if (commit == null)
        {
            Console.WriteLine($"Kein Commit mit ID {settings.commitId} gefunden.");
            return 0;
        }

        Console.WriteLine($"Commit {commit.Id} ({commit.Timestamp})");
        Console.WriteLine($"Nachricht: {commit.Message}\n");

        if (commit.Files.Count == 0)
        {
            Console.WriteLine("Keine Dateien in Commit");
            return 0;
        }

        Console.WriteLine("Enthaltene Dateien:");
        foreach (var file in commit.Files)
        {
            Console.WriteLine($"- {file.Key} => {file.Value}");
        }
        return 0;

    }
}