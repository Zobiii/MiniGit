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

            case "diff":
                ShowDiff(manager);
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
            .Select(f => Path.GetRelativePath(Directory.GetCurrentDirectory(), f))
            .ToList();

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

        var commits = manager.LoadCommits();
        commits.Add(newCommit);
        manager.SaveCommits(commits);

        string snapshotDir = Path.Join(".minigit", "snapshots");
        Directory.CreateDirectory(snapshotDir);

        foreach (var file in files)
        {
            Console.WriteLine(file);
            string hash = fileHashes[file];
            string target = Path.Join(snapshotDir, $"{Path.GetRelativePath(Directory.GetCurrentDirectory(), file)}.{hash}.bak");
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            
            File.Copy(file, target, overwrite: true);
        }

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

    static void ShowDiff(CommitManager manager)
    {
        var lastCommit = manager.LoadCommits().LastOrDefault();
        if (lastCommit == null)
        {
            Console.WriteLine("Kein Commit vorhanden");
            return;
        }

        foreach (var file in lastCommit.Files)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), file.Key);
            if (!File.Exists(path))
                continue;

            var currentLines = File.ReadAllLines(path);
            var oldHash = file.Value;
            var currentHash = FileHasher.ComputeHash(path);

            if (oldHash == currentHash)
            {
                continue;
            }

            Console.WriteLine($"\n Unterschied in: {file.Key}");

            var oldFilePath = Path.Combine(".minigit", "snapshots", $"{file.Key}.{oldHash}.bak");
            if (!File.Exists(oldFilePath))
            {
                Console.WriteLine("Alte Version nicht gespeichert - kein Vergleich möglich");
                continue;
            }

            var oldLines = File.ReadAllLines(oldFilePath);
            PrintLineDiff(oldLines, currentLines);
        }
    }

    static void PrintLineDiff(string[] oldLines, string[] newLines)
    {
        int max = Math.Max(oldLines.Length, newLines.Length);
        for (int i = 0; i < max; max++)
        {
            string oldLine = i < oldLines.Length ? oldLines[i] : "";
            string newLine = i < newLines.Length ? newLines[i] : "";

            if (oldLine != newLine)
            {
                if (!string.IsNullOrEmpty(oldLine))
                    Console.WriteLine($" - {oldLine}");

                if (!string.IsNullOrEmpty(newLine))
                    Console.WriteLine($" + {newLine}");
            }
        }
    }
}