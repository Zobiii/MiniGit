using System.Text;
using MiniGit.Commands;
using Spectre.Console.Cli;

class Program
{
    static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
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

            c.AddCommand<RestoreCommand>("restore")
                .WithDescription("Restore a file from a specific or the last commit.")
                .WithExample(new[] { "restore", "\"abc.txt\"" })
                .WithExample(new[] { "restore", "\"abc.txt\"", "\"a1b2c3d4\"" });

            c.AddCommand<VerifyCommand>("verify")
                .WithDescription("Verify if files in working directory math the last commit");

            c.AddCommand<SummaryCommand>("summary")
                .WithDescription("Show a summary of recent commits and file state");

            c.AddCommand<ExportCommand>("export")
                .WithDescription("Export snapshot of commit as .zip archive")
                .WithExample(new[] { "export", "a1b2c3d4" });

            c.AddCommand<AmendCommand>("amend")
                .WithDescription("Update the last commit's message and file state")
                .WithExample(new[] { "amend", "\"Fix typo in readme\"" })
                .WithExample(new[] { "amend" });

            c.AddCommand<LoggerCommand>("logger")
                .WithDescription("Enable/Disable logger");

            c.AddCommand<FileLoggerCommand>("flogger")
                .WithDescription("Enable/Disable file logger");

        });
        return app.Run(args);
    }
}
