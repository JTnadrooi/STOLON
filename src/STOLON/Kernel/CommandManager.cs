using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace STOLON
{
    public abstract class CommandFlag
    {

    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class CommandManager
    {
        private readonly Lazy<Shell> _shell;

        public CommandEngine Engine { get; }

        public CommandFlag? ActiveFlag { get; private set; }

        public CommandManager(CommandEngine engine, Lazy<Shell> shell)
        {
            Engine = engine;
            _shell = shell;

            ActiveFlag = null;
        }

        public void Execute(string command)
        {
            try
            {
                if (Engine.Commands.ContainsKey(command.Split(" ")[0]))
                {
                    Engine.Execute(command);
                }
                else
                {
                    _shell.Value.WriteLine($"'{command.Split(" ")[0]}' is not recognized as an internal or external command, operable program or batch file.");
                }
            }
            catch (CommandException e)
            {
                _shell.Value.WriteLine("Error: " + e.Message);
            }
        }

        public void SetFlag<TFlag>() where TFlag : CommandFlag => SetFlag(typeof(TFlag));
        public void SetFlag(Type flagType)
        {
            if (!flagType.IsAssignableTo<CommandFlag>()) throw new ArgumentException("Invalid flagtype.", nameof(flagType));

            ActiveFlag = (CommandFlag)Activator.CreateInstance(flagType)!;
        }

        public void ResetFlag()
        {
            ActiveFlag = null;
        }
    }
}