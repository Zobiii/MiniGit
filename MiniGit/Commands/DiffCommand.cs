using Spectre.Console.Cli;
using MiniGit.Utils;
using Spectre.Console;

namespace MiniGit.Commands;

public sealed class DiffCommand : Command<DiffCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var lastCommit = CommitManager.LoadCommits().LastOrDefault();
        if (lastCommit == null)
        {
            Console.WriteLine("Kein Commit vorhanden");
            return 0;
        }

        foreach (var file in lastCommit.Files)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), file.Key);
            if (!File.Exists(path))
                continue;

            var currentHash = FileHasher.ComputeHash(path);
            var oldHash = file.Value;

            if (oldHash == currentHash)
                continue;

            Console.WriteLine($"\nUnterschied in: {file.Key}");

            var oldFilePath = Path.Combine(".minigit", "snapshots", $"{lastCommit.Id}", $"{file.Key}");

            if (!File.Exists(oldFilePath))
            {
                Console.WriteLine("Alte Version nicht gespeichert - kein Vergleich möglich");
                continue;
            }

            var oldLines = File.ReadAllLines(oldFilePath);
            var currentLines = File.ReadAllLines(path);
            DiffHelper.PrintLineDiff(oldLines, currentLines);
        }
        Logger.INFO("No difference to last commit");
        return 0;
    }
}


