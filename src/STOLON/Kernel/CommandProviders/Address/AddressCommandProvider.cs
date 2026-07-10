using AsitLib.CommandLine;

namespace STOLON
{
    public class AddressCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;
        private readonly EntityDefinition[] _entityDefinitions;
        private readonly Address[] _addresses;
        private readonly WindowDependencies _windowDeps;
        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly ILogger _logger;
        private readonly IInputManager _input;
        private readonly Kernel _kernel;

        public AddressCommandProvider(
            WindowDependencies windowDeps,
            ITexture2DCollection textures,
            IFont2DCollection fonts,
            ILogger logger,
            IInputManager input,
            CommandManager commandManager,
            Shell shell,
            Kernel kernel,
            EntityDefinition[] entityDefinitions,
            Address[] addresses) : base("addr")
        {
            _commandManager = commandManager;
            _shell = shell;
            _entityDefinitions = entityDefinitions;
            _addresses = addresses;
            _windowDeps = windowDeps;
            _textures = textures;
            _fonts = fonts;
            _logger = logger;
            _input = input;
            _kernel = kernel;
        }

        private Address GetAddress(string address)
        {
            return _addresses.First(a => a.Id == address);
        }

        private Entity GetEntity(string id)
        {
            return _entityDefinitions.First(d => d.Id == id).GetDefaultEntity(new UserMoveProvider(_input, _kernel));
        }

        [KernelCommand("Set the target adress.", Id = "addr set", Aliases = ["sadr"])]
        public void SetAddress([Option(Id = "addr")] string addrId, [Option(Aliases = ["p1"])] string? player1, [Option(Aliases = ["p2"])] string? player2)
        {
            InitAddressCommandFlag flag = _commandManager.SetFlag(new InitAddressCommandFlag(GetAddress(addrId)));

            if (player1 is not null)
            {
                flag.Player1 = GetEntity(player1);
            }
            if (player2 is not null)
            {
                flag.Player2 = GetEntity(player2);
            }

            if (player1 is not null || player2 is not null)
                _shell.WriteLine($"Set target to address '{addrId}' with {flag.Player1.Definition.Id ?? "NO_SELECT"} (player1) and {flag.Player2.Definition.Id ?? "NO_SELECT"} (player2).");
            else
                _shell.WriteLine($"Set target to address '{addrId}'.");
        }

        [KernelCommand("Gets the currently active address.", Id = "addr", Aliases = ["adr"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void GetAddress()
        {
            _shell.WriteLine($"Active address: '{((InitAddressCommandFlag)_commandManager.ActiveFlag!).Address}'.");
        }

        [KernelCommand("Add entities to the selection.", Id = "selc add", Aliases = ["selc"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void AddToSelection(int playerIndex, string? id)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            if (id is null) // for the "selc" without ids "overload"
            {
                _shell.WriteLine(flag.GetPlayer(playerIndex));

                return;
            }

            try
            {
                flag.SetPlayer(playerIndex, GetEntity(id));
            }
            catch (ArgumentException e)
            {
                throw new CommandArgumentException(e.Message);
            }

            _shell.WriteLine($"Added '{id}' to player {playerIndex}'s selection.");
        }

        [KernelCommand("Removes entities from the selection.", Id = "selc rm", Aliases = ["dselc"], RequiredFlag = typeof(InitAddressCommandFlag))]
        public void ClearPlayer([Option(Aliases = ["player", "p"])] int playerIndex)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            try
            {
                flag.SetPlayer(playerIndex, null);
            }
            catch (ArgumentException e)
            {
                throw new CommandArgumentException(e.Message);
            }

            _shell.WriteLine($"Cleared player {playerIndex}'s selection.");
        }

        [KernelCommand("Initializes an address.", Id = "addr init", Aliases = ["stadr"])]
        public void InitializeAddress([Option(Id = "addr")] string? addrId, [Option(Aliases = ["p1"])] string? player1, [Option(Aliases = ["p2"])] string? player2)
        {
            if (_commandManager.ActiveFlag is not InitAddressCommandFlag flag)
            {
                if (addrId is null)
                {
                    throw new CommandArgumentException($"Missing argument {nameof(addrId)}."); // replace with helper method when I add them to AsitLib.
                }

                flag = _commandManager.SetFlag(new InitAddressCommandFlag(GetAddress(addrId)));
            }

            if (player1 is not null)
            {
                flag.Player1 = GetEntity(player1);
            }
            if (player2 is not null)
            {
                flag.Player2 = GetEntity(player2);
            }

            if (flag.Player1 is null || flag.Player2 is null)
            {
                throw new CommandException("Cannot initialize address without both players set.");
            }

            _shell.WriteWindow(new BoardWindow(_windowDeps, _logger, _shell, _commandManager, flag.Address, flag.Player1, flag.Player2));

            _shell.WriteLine($"Initialized address '{flag.Address}' with {flag.Player1.Definition.Id ?? "NO_SELECT"} (player1) and {flag.Player2.Definition.Id ?? "NO_SELECT"} (player2).");
        }
    }
}
