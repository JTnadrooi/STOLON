using AsitLib.CommandLine;

namespace STOLON
{
    public class AddressCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;
        private readonly Entity[] _entities;
        private readonly WindowDependencies _windowDeps;

        public AddressCommandProvider(CommandManager commandManager, Shell shell, Entity[] entities, WindowDependencies windowDeps) : base("addr")
        {
            _commandManager = commandManager;
            _shell = shell;
            _entities = entities;

            _windowDeps = windowDeps;
        }

        [KernelCommand("Initialize an adress.", Id = "addr set", Aliases = ["sadr"])]
        public void InitializeAddress([Option(Id = "addr")] string addrId, [Option(Id = "with")] string[] withIds)
        {
            InitAddressCommandFlag flag = _commandManager.SetFlag(new InitAddressCommandFlag(_entities, addrId));

            if (withIds.Length > 0)
            {
                foreach (string id in withIds)
                {
                    flag.Selection.Add(id);
                }
                _shell.WriteLine($"Set target to address '{addrId}' with [{withIds.ToJoinedString(", ")}].");
            }
            else
                _shell.WriteLine($"Set target to address '{addrId}'.");
        }

        [KernelCommand("Initialize an adress.", Id = "addr", Aliases = ["adr"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void GetAddress()
        {
            _shell.WriteLine($"Active address: '{((InitAddressCommandFlag)_commandManager.ActiveFlag!).Address}'.");
        }

        [KernelCommand("Select a .", Id = "selc add", Aliases = ["selc"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void AddToSelection(string[] ids)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            foreach (string id in ids)
            {
                flag.Selection.Add(id);

                _shell.WriteLine($"Added '{id}' to selection.");
            }

            _shell.WriteLine(flag.Selection);
            _shell.WriteWindow(new SelectionWindow(_windowDeps));
        }

        [KernelCommand("Select a .", Id = "selc rm", Aliases = ["dselc"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void RemoveFromSelection(string id)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            flag.Selection.Remove(id);

            _shell.WriteLine(flag.Selection);
        }
    }
}
