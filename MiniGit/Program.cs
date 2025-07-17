using MiniGit.Commands;
using MiniGit.Core;
using Spectre.Console.Cli;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(c =>
        {
            c.SetApplicationName("minigit");

            c.AddCommand<InitCommand>("init")
                .WithDescription("Create a new Minigit repository.");

            c.AddCommand<CommitCommand>("commit")
                .WithDescription("Create a new commit with tracked changes.")
                .WithExample(new[] { "commit", "\"Fix crash on login\"" });

            c.AddCommand<CheckoutCommand>("checkout")
                .WithDescription("View commit details or restore file state.");

            c.AddCommand<StatusCommand>("status")
                .WithDescription("List added, modified or deleted files since last commit.");

            c.AddCommand<DiffCommand>("diff")
                .WithDescription("Compare current files to the last commit");

            c.AddCommand<LogCommand>("log")
                .WithDescription("Show the commit history, newest first");
        });
        return app.Run(args);
    }
}
