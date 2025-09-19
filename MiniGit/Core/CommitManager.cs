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
}