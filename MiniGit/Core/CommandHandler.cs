using System.Data;
using MiniGit.Utils;

namespace MiniGit.Core
{
    public static class CommandHandler
    {
        public static void CreateSnapshot(List<string> files, string commitId)
        {
            string snapshotRoot = Path.Combine(".minigit", "snapshots", commitId);

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                var targetPath = Path.Combine(snapshotRoot, relativePath);

                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(targetDir))
                    Directory.CreateDirectory(targetDir);

                File.Copy(file, targetPath, overwrite: true);
            }
            Console.WriteLine($"📦 Snapshot gespeichert unter: {snapshotRoot}");
        }

        public static StatusInfo GetStatusInfo()
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

            var lastCommit = CommitManager.LoadCommits().LastOrDefault();
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