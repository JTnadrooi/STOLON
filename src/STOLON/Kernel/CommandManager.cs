using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public sealed class CommandManager
    {
        private readonly Shell _shell;

        public CommandEngine Engine { get; }

        public CommandManager(CommandEngine engine, Shell shell)
        {
            Engine = engine;
            _shell = shell;
        }

        public void Execute(string command)
        {
            if (Engine.Commands.ContainsKey(command.Split(" ")[0]))
            {
                Engine.Execute(command);
            }
            else
            {
                _shell.WriteLine($"'{command.Split(" ")[0]}' is not recognized as an internal or external command, operable program or batch file.");
            }
        }
    }
}
