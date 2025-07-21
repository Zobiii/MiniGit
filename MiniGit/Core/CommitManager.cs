using System.Text.Json;
using MiniGit.Utils;

public static class CommitManager
{
    private const string RepoFolder = ".minigit";
    private const string CommitsFile = "commits.json";


    public static List<Commit> LoadCommits()
    {
        Logger.INFO("Loading commits...");
        var path = Path.Combine(RepoFolder, CommitsFile);
        if (!File.Exists(path)) return new();
        string json = File.ReadAllText(path);
        List<Commit>? op = JsonSerializer.Deserialize<List<Commit>>(json);
        Logger.DEBUG($"Loaded all commits succesfully: {op?.Count ?? 0} commits");
        return op ?? new List<Commit>();
    }

    public static void SaveCommits(List<Commit> commits)
    {
        var json = JsonSerializer.Serialize(commits, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(RepoFolder, CommitsFile), json);
    }

    public static Commit? GetCommitById(string Id)
    {
        return LoadCommits().FirstOrDefault(c => c.Id.Equals(Id, StringComparison.OrdinalIgnoreCase));
    }
}