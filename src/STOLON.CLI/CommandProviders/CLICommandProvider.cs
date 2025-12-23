using AsitLib.CommandLine;

namespace STOLON.CLI
{
    public class CLICommandProvider : CommandProvider
    {
        private readonly CommandEngine _commandEngine;

        public CLICommandProvider(CommandEngine commandEngine) : base("cli")
        {
            _commandEngine = commandEngine;
        }

        [FlaggedCommand("Displays info about the STOLON.CLI.", Id = "cli", Aliases = ["info"], Flags = CommandFlags.ReadOnly)]
        public void Info()
        {
            Console.WriteLine($"Command_Count={_commandEngine.UniqueCommands.Count}");
            Console.WriteLine($"Provider_Count={_commandEngine.Providers.Count}");
            Console.WriteLine($"Is_Dev={CLI.IsDev}");
            Console.WriteLine($"STOLON_Version={STOLON.Version}");
        }

        [FlaggedCommand("Exits the program.")]
        public void Exit() => CLI.Instance.Exit();

        [FlaggedCommand("Prints the STOLON/CLI version.", Aliases = ["v"], Flags = CommandFlags.ReadOnly, IsGenericFlag = true)]
        public void Version() => Console.WriteLine(STOLON.Version);
    }
}
