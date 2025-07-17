using System.Data;
using MiniGit.Utils;

namespace MiniGit.Core
{
    public static class CommandHandler
    {
        public static void CreateSnapshot(List<string> files, Dictionary<string, string> fileHashes)
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