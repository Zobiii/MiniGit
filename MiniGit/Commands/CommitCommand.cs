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
        Logger.DEBUG("Command commit executed");

        string message = settings.MessageArgs.Length > 0 ? string.Join(" ", settings.MessageArgs) : "Kein Commit-Text";

        var files = CommandHandler.GetFilesToProcess(Environment.CurrentDirectory).ToList();
        Logger.DEBUG("Recieved files for further processing");

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
        Logger.INFO("Submitted new commit");

        CommitManager.SaveCommits(commits);
        Logger.INFO($"Commit: {newCommit.Id} saved");

        Output.Console($"Commit erstellt mit ID: {newCommit.Id}");

        CommandHandler.CreateSnapshot(files, newCommit.Id);
        Logger.DEBUG($"Created new snapshot folder: {newCommit.Id}");

        return 0;
    }
}