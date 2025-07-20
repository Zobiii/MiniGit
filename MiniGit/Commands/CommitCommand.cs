using Spectre.Console.Cli;
using MiniGit.Utils;
using MiniGit.Core;


namespace MiniGit.Commands;

public sealed class CommitCommand : Command<CommitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<message>")]
        public string[] MessageArgs { get; set; } = [];
    }

    public override int Execute(CommandContext context, Settings settings)
    {

        string message = settings.MessageArgs.Length > 0 ? string.Join(" ", settings.MessageArgs) : "Kein Commit-Text";

        var ignorePatterns = FileHelper.LoadIgnorePatterns();

        Console.WriteLine("Minigitignore geladen:");
        foreach (var pattern in ignorePatterns)
        {
            Console.WriteLine("  → " + pattern);
        }

        var files = CommandHandler.GetFilesToProcess(Environment.CurrentDirectory).ToList();

        var fileHashes = files.ToDictionary(
            path => Path.GetRelativePath(Directory.GetCurrentDirectory(), path),
            path => FileHasher.ComputeHash(Path.GetRelativePath(Directory.GetCurrentDirectory(), path))
        );

        var newCommit = new Commit
        {
            Id = Guid.NewGuid().ToString()[..8],
            Timestamp = DateTime.Now,
            Files = fileHashes,
            Message = message
        };



        var commits = CommitManager.LoadCommits();
        commits.Add(newCommit);
        CommitManager.SaveCommits(commits);

        CommandHandler.CreateSnapshot(files, newCommit.Id);

        Console.WriteLine($"Commit erstellt mit ID: {newCommit.Id}");

        return 0;
    }
}