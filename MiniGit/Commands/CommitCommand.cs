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



        bool success = CommitManager.AtomicCommit(newCommit, files);
        if (!success)
        {
            Output.Console("Fehler: Atomarer Commit fehlgeschlagen(Repository gesperrt oder IO - Fehler ?)");
            return 1;
        }

        Output.Console($"Commit erstellt mit ID: {newCommit.Id}");
        Logger.INFO($"Atomic commit completed for ID: {newCommit.Id}");

        return 0;
    }
}