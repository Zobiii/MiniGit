using System.Text.Json;
using System.Xml.Serialization;

public class CommitManager
{
    private const string RepoFolder = ".minigit";
    private const string CommitsFile = "commits.json";

    public void Init()
    {
        if (!Directory.Exists(RepoFolder))
        {
            Directory.CreateDirectory(RepoFolder);
            File.WriteAllText(Path.Combine(RepoFolder, CommitsFile), "[]");
            Console.WriteLine("MiniGit-Repository initialisiert.");
        }
        else
        {
            Console.WriteLine("Repository existiert bereits.");
        }
    }

    public List<Commit> LoadCommits()
    {
        var path = Path.Combine(RepoFolder, CommitsFile);
        if (!File.Exists(path)) return new();
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Commit>>(json);
    }

    public void SaveCommits(List<Commit> commits)
    {
        var json = JsonSerializer.Serialize(commits, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(RepoFolder, CommitsFile), json);
    }

    public Commit? GetCommitById(string Id)
    {
        return LoadCommits().FirstOrDefault(c => c.Id.Equals(Id, StringComparison.OrdinalIgnoreCase));
    }
}