using System.Data;
using MiniGit.Utils;

namespace MiniGit.Core
{
    public static class CommandHandler
    {
        public static StatusInfo GetStatusInfo()
        {
            Logger.INFO("Getting working directory status...");

            var currentFiles = GetFilesToProcess(Environment.CurrentDirectory).ToList();

            Logger.DEBUG($"Remaining {currentFiles.Count} files after filtering ignored patterns");

            var currentHashes = currentFiles.ToDictionary(
                rel => rel,
                rel =>
                {
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), rel);
                    string hash = FileHasher.ComputeHash(rel);
                    return hash;
                }
            );

            var lastCommit = CommitManager.LoadCommits().LastOrDefault();
            if (lastCommit == null)
            {
                Logger.WARN("No commits found. Status cannot be computed");
                return new StatusInfo();
            }

            var previousFiles = lastCommit.Files;
            Logger.INFO($"Last commit has {previousFiles.Count} tracked files");

            var newFiles = currentHashes.Keys.Except(previousFiles.Keys).ToList();
            foreach (var nf in newFiles)
                Logger.INFO($"New file detected: {nf}");

            var deletedFiles = previousFiles.Keys.Except(currentHashes.Keys).ToList();
            foreach (var df in deletedFiles)
                Logger.WARN($"Deleted file detected: {df}");

            var changedFiles = currentHashes.Keys
                .Where(f => previousFiles.ContainsKey(f) && previousFiles[f] != currentHashes[f])
                .ToList();
            foreach (var cf in changedFiles)
                Logger.INFO($"Modified file detected: {cf}");


            Logger.INFO($"Status check complete: {newFiles.Count} new, {changedFiles.Count} modified, {deletedFiles.Count} deleted.");


            return new StatusInfo
            {
                NewFiles = newFiles,
                ChangedFiles = changedFiles,
                DeletedFiles = deletedFiles
            };
        }

        public static IEnumerable<string> GetFilesToProcess(string workingDir)
        {
            var ignore = FileHelper.LoadIgnorePatterns();
            List<string> result = [];

            foreach (var file in Directory.EnumerateFiles(workingDir, "*.*", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(workingDir, file);

                if (!FileHelper.ShouldIgnore(relPath, ignore))
                {
                    Logger.DEBUG($"Passing {relPath} for futher processing...");
                    result.Add(relPath);
                }
            }
            return result;
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