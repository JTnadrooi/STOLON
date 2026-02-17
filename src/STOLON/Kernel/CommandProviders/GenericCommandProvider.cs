using AsitLib.CommandLine;

namespace STOLON
{
    public class GenericCommandProvider : CommandProvider
    {
        private readonly CommandEngine _commandEngine;
        private readonly Shell _shell;

        public GenericCommandProvider(Shell shell, CommandEngine commandEngine) : base("generic") // way too long id but it shoulden't be typed by the user.
        {
            _commandEngine = commandEngine;
            _shell = shell;
        }

        [KernelCommand("Prints the STOLON version.", Aliases = ["v"], IsGenericFlag = true)]
        public void Version() => _shell.WriteLine(STOLON.Version);
    }
}
