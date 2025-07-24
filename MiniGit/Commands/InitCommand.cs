using MiniGit.Utils;
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
            Logger.SetEnabled(false);
            Logger.INFO("Creating repository folder...");
            Logger.INFO("Creating logger conifg...");

            FileLogger.SetEnabled(false);
            Logger.INFO("Creating filelogger config...");

            Directory.CreateDirectory(RepoFolder);
            File.WriteAllText(Path.Combine(RepoFolder, CommitsFile), "[]");
            Logger.INFO("Creating file commits.json...");

            string snapshotRoot = Path.Combine(".minigit", "snapshots");
            Directory.CreateDirectory(snapshotRoot);
            Logger.INFO("Creating snapshot folder...");

            AnsiConsole.Status()
                .Start("Creating repository...", ctx =>
                {
                    // Simulate some work
                    Thread.Sleep(2500);

                    // Update the status and spinner
                    ctx.Status("Wird Abgeschlossen...");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    // Simulate some work
                    Output.Console("Repository erstellt.");
                    Thread.Sleep(2500);
                });
        }
        else
        {
            Logger.DEBUG("Repository files already exist, return...");
            AnsiConsole.Status()
                .Start("Creating repository...", ctx =>
                {
                    // Simulate some work
                    Thread.Sleep(2500);

                    // Update the status and spinner
                    ctx.Status("Wird Abgebrochen...");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("red"));

                    // Simulate some work
                    Output.Console("Repository existiert bereits.");
                    Thread.Sleep(2500);
                });
        }
        return 0;

    }
}


