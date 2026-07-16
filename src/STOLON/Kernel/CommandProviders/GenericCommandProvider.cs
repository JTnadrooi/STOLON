using AsitLib.CommandLine;
using System.ComponentModel.DataAnnotations;

namespace STOLON
{
    public class GenericCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;
        private readonly Shell _shell;
        private readonly EntityDefinition[] _entityDefinitions;

        public GenericCommandProvider(CommandManager commandManager, Shell shell, EntityDefinition[] entityDefinitions) : base("generic") // way too long id but it shoulden't be typed by the user.
        {
            _commandManager = commandManager;
            _shell = shell;
            _entityDefinitions = entityDefinitions;
        }

        [KernelCommand("Prints the reality version.", Aliases = ["v"], Id = "version", IsGenericFlag = true)] // not the game version, lore version.
        public void GetVersion() => _shell.WriteLine($"revise--1236979249_sto+a");

        [KernelCommand(".", Id = "flag", IsExternal = true)]
        public void GetActiveFlag()
        {
            _shell.WriteLine(_commandManager.ActiveFlag.ToString() ?? StringHelpers.NULL_STRING);
        }

        [KernelCommand(".", Id = "entity", Aliases = ["e"])]
        public void ShowEntity([EntityId] string id)
        {
            _shell.WriteTexture(_entityDefinitions.First(e => e.Id == id).Mipmaps[128]);
        }
    }
}
