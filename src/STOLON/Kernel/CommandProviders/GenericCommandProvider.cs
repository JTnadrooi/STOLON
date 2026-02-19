using AsitLib.CommandLine;

namespace STOLON
{
    public class GenericCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;

        public GenericCommandProvider(Shell shell, CommandManager commandManager) : base("generic") // way too long id but it shoulden't be typed by the user.
        {
            _commandManager = commandManager;
            _shell = shell;
        }

        [KernelCommand("Prints the STOLON version.", Aliases = ["v"], Id = "version", IsGenericFlag = true)]
        public void GetVersion() => _shell.WriteLine(STOLON.Version);

    }
}
