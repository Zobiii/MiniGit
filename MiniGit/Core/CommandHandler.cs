using Minigit.Utils;
using MiniGit.Utils;

namespace MiniGit.Core
{
    public class CommandHandler
    {
        private readonly CommitManager _commitManager;

        public CommandHandler(CommitManager commitManager)
        {
            _commitManager = commitManager;
        }

        public void HandleCommit(string[] messageArgs)
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

            var commits = _commitManager.LoadCommits();
            commits.Add(newCommit);
            _commitManager.SaveCommits(commits);

            CreateSnapshot(files, fileHashes);

            Console.WriteLine($"Commit erstellt mit ID: {newCommit.Id}");
        }

        public void HandleLog()
        {
            var commits = _commitManager.LoadCommits();
            foreach (var c in commits.OrderByDescending(x => x.Timestamp))
            {
                Console.WriteLine($"{c.Id} - {c.Timestamp} - {c.Message}");
            }
        }

        public void HandleCheckout(string commitId)
        {
            var commit = _commitManager.GetCommitById(commitId);
            if (commit == null)
            {
                Console.WriteLine($"Kein Commit mit ID {commitId} gefunden.");
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

        public void HandleStatus()
        {
            var statusInfo = GetStatusInfo();
            
            if (!statusInfo.HasChanges)
            {
                Console.WriteLine("✅ Keine Änderungen seit letztem Commit.");
                return;
            }

            if (statusInfo.NewFiles.Any())
            {
                Console.WriteLine("\n🆕 Neue Dateien:");
                foreach (var f in statusInfo.NewFiles) Console.WriteLine("  + " + f);
            }

            if (statusInfo.ChangedFiles.Any())
            {
                Console.WriteLine("\n✏️  Geänderte Dateien:");
                foreach (var f in statusInfo.ChangedFiles) Console.WriteLine("  ~ " + f);
            }

            if (statusInfo.DeletedFiles.Any())
            {
                Console.WriteLine("\n❌ Gelöschte Dateien:");
                foreach (var f in statusInfo.DeletedFiles) Console.WriteLine("  - " + f);
            }
        }

        public void HandleDiff()
        {
            var lastCommit = _commitManager.LoadCommits().LastOrDefault();
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

                var currentHash = FileHasher.ComputeHash(path);
                var oldHash = file.Value;

                if (oldHash == currentHash)
                    continue;

                Console.WriteLine($"\n Unterschied in: {file.Key}");

                var oldFilePath = Path.Combine(".minigit", "snapshots", $"{file.Key}.{oldHash}.bak");
                if (!File.Exists(oldFilePath))
                {
                    Console.WriteLine("Alte Version nicht gespeichert - kein Vergleich möglich");
                    continue;
                }

                var oldLines = File.ReadAllLines(oldFilePath);
                var currentLines = File.ReadAllLines(path);
                DiffHelper.PrintLineDiff(oldLines, currentLines);
            }
        }

        private void CreateSnapshot(List<string> files, Dictionary<string, string> fileHashes)
        {
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
        }

        private StatusInfo GetStatusInfo()
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

            var lastCommit = _commitManager.LoadCommits().LastOrDefault();
            if (lastCommit == null)
            {
                return new StatusInfo();
            }

            var previousFiles = lastCommit.Files;

            var newFiles = currentHashes.Keys.Except(previousFiles.Keys).ToList();
            var deletedFiles = previousFiles.Keys.Except(currentHashes.Keys).ToList();
            var changedFiles = currentHashes.Keys
                .Where(f => previousFiles.ContainsKey(f) && previousFiles[f] != currentHashes[f])
                .ToList();

            return new StatusInfo
            {
                NewFiles = newFiles,
                ChangedFiles = changedFiles,
                DeletedFiles = deletedFiles
            };
        }
    }

    public class StatusInfo
    {
        public List<string> NewFiles { get; set; } = new();
        public List<string> ChangedFiles { get; set; } = new();
        public List<string> DeletedFiles { get; set; } = new();
        
        public bool HasChanges => NewFiles.Any() || ChangedFiles.Any() || DeletedFiles.Any();
    }
}
