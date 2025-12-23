using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class SourceCommandProvider : CommandGroup
    {
        private readonly IRichLogger _logger;

        public SourceCommandProvider(IRichLogger logger) : base("src", CLI.InfoFactory, nameOfMainMethod: nameof(Main))
        {
            _logger = logger;
        }

        [FlaggedCommand("Opens the local source code directory.", Flags = CommandFlags.DevOnly, PassingPolicies = OptionPassingPolicies.Named)]
        public void Main(string? subDir = null)
        {
            CLIHelpers.OpenDirectory(CLI.SourcePath! + subDir switch
            {
                "sl" => "STOLON\\",
                "cli" => "STOLON.CLI\\",
                null => string.Empty,
                _ => throw new ArgumentException("Accepted values; sl, cli", nameof(subDir))
            });
            _logger.Log($"Opened {(subDir == null ? string.Empty : $"'{subDir} '")}source directory.");
        }

        [FlaggedCommand("Prints the local source code directory path.", Flags = CommandFlags.DevOnly | CommandFlags.ReadOnly)]
        public void Path()
        {
            Console.WriteLine(CLI.SourcePath);
        }
    }
}
