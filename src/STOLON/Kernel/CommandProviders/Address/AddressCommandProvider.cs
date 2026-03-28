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

            flag.Selection.AddRange(withIds);

            if (withIds.Length > 0)
                _shell.WriteLine($"Set target to address '{addrId}' with [{withIds.Select(id => $"'{id}'").ToJoinedString(", ")}].");
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

            if (ids.Length == 0) // for the "selc" without input "overload"
            {
                _shell.WriteLine(flag.Selection);

                return;
            }

            try
            {
                flag.Selection.AddRange(ids);
            }
            catch (ArgumentException e)
            {
                throw new CommandArgumentException(e.Message);
            }

            if (ids.Length > 0)
                _shell.WriteLine($"Added [{ids.Select(id => $"'{id}'").ToJoinedString(", ")}] to selection.");
            else
                _shell.WriteLine($"Added '{ids[0]}' to selection.");

            //_shell.WriteLine(flag.Selection);
            //_shell.WriteWindow(new SelectionWindow(_windowDeps));
        }

        [KernelCommand("Select a .", Id = "selc rm", Aliases = ["dselc"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void RemoveFromSelection(string[] ids)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            if (ids.Length == 0)
            {
                throw new CommandArgumentException("You must provide at least one id.");
            }

            try
            {
                flag.Selection.RemoveRange(ids);
            }
            catch (ArgumentException e)
            {
                throw new CommandArgumentException(e.Message);
            }

            if (ids.Length > 0)
                _shell.WriteLine($"Removed [{ids.Select(id => $"'{id}'").ToJoinedString(", ")}] from selection.");
            else
                _shell.WriteLine($"Removed '{ids[0]}' from selection.");
        }
    }
}
