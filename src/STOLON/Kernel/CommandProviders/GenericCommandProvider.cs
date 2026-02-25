using AsitLib.CommandLine;

namespace STOLON
{
    public sealed class InitAddressCommandFlag : CommandFlag
    {

    }

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

        [KernelCommand(".", Id = "addr")]
        public void InitAddress() => _commandManager.SetFlag<InitAddressCommandFlag>();

        [KernelCommand(".", Id = "flag", RequiredFlag = typeof(InitAddressCommandFlag))]
        public void GetActiveFlag() => _shell.WriteLine(_commandManager.ActiveFlag);
    }
}
