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
            c.SetApplicationName("Hure");
            c.AddCommand<InitCommand>("init");
            c.AddCommand<CommitCommand>("commit");
            c.AddCommand<CheckoutCommand>("checkout");
            c.AddCommand<StatusCommand>("status");
            c.AddCommand<DiffCommand>("diff");
        });
        return app.Run(args);
    }
}
