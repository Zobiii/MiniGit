public class Commit
{
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Files { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}