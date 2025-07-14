using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using Minigit.Utils;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Befehle: init | commit | log");
            return;
        }

        var manager = new CommitManager();

        switch (args[0])
        {
            case "init":
                manager.Init();
                break;

            case "log":
                var commits = manager.LoadCommits();
                foreach (var c in commits.OrderByDescending(x => x.Timestamp))
                {
                    Console.WriteLine($"{c.Id} - {c.Timestamp} - {c.Message}");
                }
                break;

            case "commit":
                CommitsFile(manager, args.Skip(1).ToArray());
                break;

            case "checkout":
                if (args.Length < 2)
                {
                    Console.WriteLine("Bitte Commit-ID angeben: minigit checkout <id>");
                    return;
                }
                ShowCommitInfo(manager, args[1]);
                break;

            case "status":
                ShowStatus(manager);
                break;

            default:
                Console.WriteLine("Unbekannter Befehl.");
                break;
        }
    }

    static void CommitsFile(CommitManager manager, string[] messageArgs)
    {
        string message = messageArgs.Length > 0 ? string.Join(" ", messageArgs) : "Kein Commit-Text";

        var ignorePatterns = FileHelper.LoadIgnorePatterns();

        Console.WriteLine("Minigitignore geladen:");
        foreach (var pattern in ignorePatterns)
        {
            Console.WriteLine("  → " + pattern);
        }


        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)
            .Where(f => !FileHelper.ShouldIgnore(f, ignorePatterns))
            .ToList();

        var fileHashes = files.ToDictionary(
            path => Path.GetRelativePath(Directory.GetCurrentDirectory(), path),
            path => FileHasher.ComputeHash(path)
        );

        var newCommit = new Commit
        {
            Id = Guid.NewGuid().ToString()[..8],
            Timestamp = DateTime.Now,
            Files = fileHashes,
            Message = message
        };

        var commits = manager.LoadCommits();
        commits.Add(newCommit);
        manager.SaveCommits(commits);

        Console.WriteLine($"Commit erstellt mit ID: {newCommit.Id}");
    }

    static void ShowCommitInfo(CommitManager manager, string id)
    {
        var commit = manager.GetCommitById(id);
        if (commit == null)
        {
            Console.WriteLine($"Kein Commit mit ID {id} gefunden.");
            return;
        }

        Console.WriteLine($"Commit {commit.Id} ({commit.Timestamp})");
        Console.WriteLine($"Nachricht: {commit.Message}\n");

        if (commit.Files.Count == 0)
        {
            Console.WriteLine("Keine Dateien in Commit");
            return;
        }

        Console.WriteLine("Enthaltene Dateien:");
        foreach (var file in commit.Files)
        {
            Console.WriteLine($"- {file.Key} => {file.Value}");
        }
    }

    static void ShowStatus(CommitManager manager)
    {
        var ignorePatterns = FileHelper.LoadIgnorePatterns();
        var allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories);

        var currentFiles = allFiles
            .Select(path => Path.GetRelativePath(Directory.GetCurrentDirectory(), path))
            .Where(rel => !FileHelper.ShouldIgnore(rel, ignorePatterns))
            .ToList();

        var currentHashes = currentFiles.ToDictionary(
            rel => rel,
            rel => FileHasher.ComputeHash(Path.Combine(Directory.GetCurrentDirectory(), rel))
        );

        var lastCommit = manager.LoadCommits().LastOrDefault();
        if (lastCommit == null)
        {
            Console.WriteLine("Kein Commit vorhanden");
            return;
        }

        var previousFiles = lastCommit.Files;

        var newFiles = currentHashes.Keys.Except(previousFiles.Keys).ToList();
        var deletedFiles = previousFiles.Keys.Except(currentHashes.Keys).ToList();
        var changedFiles = currentHashes.Keys
            .Where(f => previousFiles.ContainsKey(f) && previousFiles[f] != currentHashes[f])
            .ToList();

        if (!newFiles.Any() && !changedFiles.Any() && !deletedFiles.Any())
        {
            Console.WriteLine("✅ Keine Änderungen seit letztem Commit.");
            return;
        }

        if (newFiles.Any())
        {
            Console.WriteLine("\n🆕 Neue Dateien:");
            foreach (var f in newFiles) Console.WriteLine("  + " + f);
        }

        if (changedFiles.Any())
        {
            Console.WriteLine("\n✏️  Geänderte Dateien:");
            foreach (var f in changedFiles) Console.WriteLine("  ~ " + f);
        }

        if (deletedFiles.Any())
        {
            Console.WriteLine("\n❌ Gelöschte Dateien:");
            foreach (var f in deletedFiles) Console.WriteLine("  - " + f);
        }
    }
}