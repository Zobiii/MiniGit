using Spectre.Console;

namespace MiniGit.Utils
{
    public static class Output
    {
        private const string Prefix = "[orange1][[MiniGit]][/]";

        public static void Console(string message)
        {
            AnsiConsole.MarkupLine($"{Prefix} {message}");
        }
    }
}