using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class DevCommandProvider : CommandGroup
    {
        public DevCommandProvider() : base("dev", CLI.InfoFactory, nameOfMainMethod: nameof(Main)) { }

        [FlaggedCommand($"Prints a value indicating if the {CLI.BuildInfoDirectory} directory is found and valid.", Flags = CommandFlags.ReadOnly)]
        public void Main()
        {
            Console.WriteLine(CLI.IsDev);
        }
    }
}
