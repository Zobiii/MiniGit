using Spectre.Console;
using Spectre.Console.Cli;

namespace MiniGit.Commands;

public sealed class InitCommand : Command<InitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        const string RepoFolder = ".minigit";
        const string CommitsFile = "commits.json";


        if (!Directory.Exists(RepoFolder))
        {
            Directory.CreateDirectory(RepoFolder);
            File.WriteAllText(Path.Combine(RepoFolder, CommitsFile), "[]");
            AnsiConsole.Status()
                .Start("Thinking...", ctx =>
                {
                    // Simulate some work
                    AnsiConsole.MarkupLine(".minigit wird erstellt...");
                    Thread.Sleep(3000);
                    AnsiConsole.MarkupLine("commits.json wird erstellt...");
                    Thread.Sleep(3000);

                    // Update the status and spinner
                    ctx.Status("Wird Abgeschlossen...");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    // Simulate some work
                    AnsiConsole.MarkupLine("Repository erstellt.");
                    Thread.Sleep(6000);
                });
        }
        else
        {
            Console.WriteLine("Repository existiert bereits.");
        }
        return 0;

    }
}


