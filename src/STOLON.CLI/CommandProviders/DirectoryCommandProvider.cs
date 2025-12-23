using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class DirectoryCommandProvider : CommandGroup
    {
        private readonly IRichLogger _logger;

        public DirectoryCommandProvider(IRichLogger logger) : base("dir", CLI.InfoFactory, nameOfMainMethod: nameof(Main))
        {
            _logger = logger;
        }

        [FlaggedCommand("Opens the directory where STOLON is located.")]
        public void Main()
        {
            CLIHelpers.OpenDirectory(AppDomain.CurrentDomain.BaseDirectory);
            _logger.Log("Opened STOLON directory.");
        }

        [FlaggedCommand("Prints the path of the directory where STOLON is located.", Flags = CommandFlags.ReadOnly)]
        public void Path()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
