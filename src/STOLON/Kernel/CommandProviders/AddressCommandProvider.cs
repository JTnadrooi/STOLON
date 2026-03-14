using AsitLib.CommandLine;

namespace STOLON
{
    public sealed class InitAddressCommandFlag : CommandFlag
    {
        public HashSet<string> Selection { get; } = new HashSet<string>();

        public string Address { get; set; }

        public InitAddressCommandFlag(string address)
        {
            Address = address;
        }
    }

    public class AddressCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;
        private readonly Entity[] _entities;

        public AddressCommandProvider(CommandManager commandManager, Shell shell, Entity[] entities) : base("addr") // way too long id but it shoulden't be typed by the user.
        {
            _commandManager = commandManager;
            _shell = shell;
            _entities = entities;
        }

        [KernelCommand("Initialize an adress.", Id = "addr")]
        public void InitializeAddress(string id)
        {
            InitAddressCommandFlag flag = _commandManager.SetFlag(new InitAddressCommandFlag(id));
        }

        [KernelCommand("Select a .", Id = "selc", RequiredFlag = typeof(InitAddressCommandFlag))]
        public void SelectEntity(string entityName)
        {
            InitAddressCommandFlag flag = (InitAddressCommandFlag)_commandManager.ActiveFlag!;

            flag.Selection.Add(entityName);

            _shell.WriteLine(flag.Selection.ToJoinedString(", "));

            _shell.WriteTexture(_entities.First(e => e.Id == entityName).Mipmaps[128]);
        }
    }
}
