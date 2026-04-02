using AsitLib.CommandLine;

namespace STOLON
{
    public class AddressCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;
        private readonly Entity[] _entities;
        private readonly Address[] _addresses;
        private readonly WindowDependencies _windowDeps;
        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly ILogger _logger;
        private readonly IInputManager _input;

        public AddressCommandProvider(
            WindowDependencies windowDeps,
            ITexture2DCollection textures,
            IFont2DCollection fonts,
            ILogger logger,
            IInputManager input,
            CommandManager commandManager,
            Shell shell,
            Entity[] entities,
            Address[] addresses) : base("addr")
        {
            _commandManager = commandManager;
            _shell = shell;
            _entities = entities;
            _addresses = addresses;
            _windowDeps = windowDeps;
            _textures = textures;
            _fonts = fonts;
            _logger = logger;
            _input = input;
        }

        private Address GetAddress(string address)
        {
            return _addresses.First(a => a.Id == address);
        }

        [KernelCommand("Set the target adress.", Id = "addr set", Aliases = ["sadr"])]
        public void SetAddress([Option(Id = "addr")] string addrId, [Option(Id = "with")] string[] withIds)
        {
            InitAddressCommandFlag flag = _commandManager.SetFlag(new InitAddressCommandFlag(_entities, GetAddress(addrId)));

            flag.Selection.AddRange(withIds);

            if (withIds.Length > 0)
                _shell.WriteLine($"Set target to address '{addrId}' with [{withIds.Select(id => $"'{id}'").ToJoinedString(", ")}].");
            else
                _shell.WriteLine($"Set target to address '{addrId}'.");
        }

        [KernelCommand("Gets the currently active address.", Id = "addr", Aliases = ["adr"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void GetAddress()
        {
            _shell.WriteLine($"Active address: '{((InitAddressCommandFlag)_commandManager.ActiveFlag!).Address}'.");
        }

        [KernelCommand("Add entities to the selection.", Id = "selc add", Aliases = ["selc"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void AddToSelection(string[] ids)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            if (ids.Length == 0) // for the "selc" without ids "overload"
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

        [KernelCommand("Removes entities from the selection.", Id = "selc rm", Aliases = ["dselc"], RequiredFlag = typeof(InitAddressCommandFlag))]
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

        [KernelCommand("Initializes an address.", Id = "addr init", Aliases = ["stadr"])]
        public void InitializeAddress([Option(Id = "addr")] string? addrId, [Option(Id = "with")] string[] withIds)
        {
            if (_commandManager.ActiveFlag is not InitAddressCommandFlag flag)
            {
                if (addrId is null)
                {
                    throw new CommandArgumentException($"Missing argument {nameof(addrId)}."); // replace with helper method when I add them to AsitLib.
                }

                flag = _commandManager.SetFlag(new InitAddressCommandFlag(_entities, GetAddress(addrId)));
            }

            if (withIds.Length > 0)
                flag.Selection.AddRange(withIds);

            _shell.WriteWindow(new BoardWindow(_windowDeps, _textures, _fonts, _input, _logger, flag.Address));

            _shell.WriteLine($"Initialized address '{flag.Address}' with [{flag.Selection.Entries.Keys.Select(entry => $"'{entry}'").ToJoinedString(", ")}].");
        }
    }
}
