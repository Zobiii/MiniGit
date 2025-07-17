using System.Text.Json;

public static class CommitManager
{
    private const string RepoFolder = ".minigit";
    private const string CommitsFile = "commits.json";


    public static List<Commit> LoadCommits()
    {
        var path = Path.Combine(RepoFolder, CommitsFile);
        if (!File.Exists(path)) return new();
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Commit>>(json) ?? new List<Commit>();
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