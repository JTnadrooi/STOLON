using AsitLib.CommandLine;

namespace STOLON
{
    public class GenericCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;

        public GenericCommandProvider(CommandManager commandManager, Shell shell) : base("generic") // way too long id but it shoulden't be typed by the user.
        {
            _commandManager = commandManager;
            _shell = shell;
        }

        [KernelCommand("Prints the reality version.", Aliases = ["v"], Id = "version", IsGenericFlag = true)] // not the game version, lore version.
        public void GetVersion() => _shell.WriteLine($"revise--1236979249_sto+a");

        [KernelCommand(".", Id = "flag", RequiredFlag = typeof(InitAddressCommandFlag), IsExternal = true)]
        public void GetActiveFlag()
        {
            _shell.WriteLine(_commandManager.ActiveFlag);
        }
    }
}
