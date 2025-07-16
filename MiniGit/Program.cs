using MiniGit.Core;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var commitManager = new CommitManager();
        var commandHandler = new CommandHandler(commitManager);

        switch (args[0].ToLower())
        {
            case "init":
                commitManager.Init();
                break;

            case "log":
                commandHandler.HandleLog();
                break;

            case "commit":
                commandHandler.HandleCommit(args.Skip(1).ToArray());
                break;

            case "checkout":
                if (args.Length < 2)
                {
                    Console.WriteLine("Bitte Commit-ID angeben: minigit checkout <id>");
                    return;
                }
                commandHandler.HandleCheckout(args[1]);
                break;

            case "status":
                commandHandler.HandleStatus();
                break;

            case "diff":
                commandHandler.HandleDiff();
                break;

            case "help":
            case "-h":
            case "--help":
                ShowHelp();
                break;

            default:
                Console.WriteLine($"Unbekannter Befehl: {args[0]}");
                ShowHelp();
                break;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("MiniGit - Ein einfaches Versionskontrollsystem");
        Console.WriteLine();
        Console.WriteLine("Verfügbare Befehle:");
        Console.WriteLine("  init     - Repository initialisieren");
        Console.WriteLine("  commit   - Dateien committen (optional: Nachricht)");
        Console.WriteLine("  log      - Commit-Historie anzeigen");
        Console.WriteLine("  status   - Aktuellen Status anzeigen");
        Console.WriteLine("  diff     - Unterschiede seit letztem Commit anzeigen");
        Console.WriteLine("  checkout - Commit-Informationen anzeigen");
        Console.WriteLine("  help     - Diese Hilfe anzeigen");
    }
}