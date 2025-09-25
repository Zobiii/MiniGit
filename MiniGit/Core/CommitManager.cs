using System.Text.Json;
using MiniGit.Utils;
using MiniGit.Core;

public static class CommitManager
{
    private const string RepoFolder = ".minigit";
    private const string CommitsFile = "commits.json";


    public static List<Commit> LoadCommits()
    {
        Logger.INFO("Loading all commits...");
        var path = Path.Combine(RepoFolder, CommitsFile);
        if (!File.Exists(path))
        {
            Logger.WARN($"Folder '{RepoFolder}' or file '{CommitsFile}' does not exist, creating one...");
            return new();
        }
        string json = File.ReadAllText(path);
        List<Commit>? op = JsonSerializer.Deserialize<List<Commit>>(json);
        Logger.DEBUG($"Loaded all commits succesfully: {op?.Count ?? 0} commits");
        return op ?? new List<Commit>();
    }


    public static bool SaveCommits(List<Commit> commits)
    {
        Logger.INFO($"Started saving commits: {commits.Count}");

        return RepositoryLock.ExecuteWithLock(() =>
        {
            var json = JsonSerializer.Serialize(commits, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(RepoFolder, CommitsFile), json);
            Logger.DEBUG($"Saved '{commits.Count}' commits at '{CommitsFile}'");
        });
    }


    public static Commit? GetCommitById(string Id)
    {
        return LoadCommits().FirstOrDefault(c => c.Id.Equals(Id, StringComparison.OrdinalIgnoreCase));
    }

    public static bool AtomicCommit(Commit newCommit, List<string> filesToSnapshot)
    {
        var (success, result) = RepositoryLock.ExecuteWithLock<bool>(() =>
        {
            Logger.INFO($"Starting atomic commit for ID: {newCommit.Id}");

            using var transaction = new CommitTransaction(newCommit.Id);

            var commits = LoadCommits();
            commits.Add(newCommit);

            if (!transaction.Prepare(commits, filesToSnapshot))
            {
                Logger.ERROR("Atomic commit failed during prepare phase");
                return false;
            }

            if (!transaction.Commit())
            {
                Logger.ERROR("Atomic commit failed during commit phase");
                return false;
            }

            Logger.INFO($"Atomic commit completed successfully for ID: {newCommit.Id}");
            return true;
        });

        return success && result;
    }

    public static bool AtomicAmend(Commit amendedCommit, List<string> filesToSnapshot)
    {
        var (success, result) = RepositoryLock.ExecuteWithLock<bool>(() =>
        {
            Logger.INFO($"Starting atomic amend for ID: {amendedCommit.Id}");

            using var transaction = new CommitTransaction(amendedCommit.Id);

            var commits = LoadCommits();
            if (commits.Count > 0)
            {
                commits[commits.Count - 1] = amendedCommit;
            }
            else
            {
                Logger.ERROR("No commits to amend");
                return false;
            }

            if (!transaction.Prepare(commits, filesToSnapshot))
            {
                Logger.ERROR("Atomic amend failed during prepare phase");
                return false;
            }

            if (!transaction.Commit())
            {
                Logger.ERROR("Atomic amend failed during commit phase");
                return false;
            }

            Logger.INFO($"Atomic amend completed successfully for ID: {amendedCommit.Id}");
            return true;
        });

        return success && result;
    }
}