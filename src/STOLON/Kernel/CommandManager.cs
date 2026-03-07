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

        /// <summary>
        /// Gets the currently active <see cref="CommandFlag"/> or <see langword="null"/> if no flag is active.
        /// </summary>
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
                if (Engine.Commands.ContainsKey(command.Split(" ")[0])) // to be improved when asitlib adapts better exceptions.
                {
                    CommandResult result = Engine.Execute(command);

                    if (!result.IsVoid)
                    {
                        _shell.Value.WriteLine(result.ToOutputString());
                    }
                }
                else
                {
                    throw new CommandException($"'{command.Split(" ")[0]}' is not recognized as an internal or external command, operable program or batch file.");
                }
            }
            catch (CommandException e)
            {
                _shell.Value.WriteLine(e.Message);
            }
        }

        public TFlag SetFlag<TFlag>(TFlag flag) where TFlag : CommandFlag
        {
            ActiveFlag = flag;

            return (TFlag)ActiveFlag;
        }

        public TFlag SetFlag<TFlag>() where TFlag : CommandFlag, new() => (TFlag)SetFlag(typeof(TFlag));
        public object SetFlag(Type flagType)
        {
            if (!flagType.IsAssignableTo<CommandFlag>()) throw new ArgumentException("Invalid flagtype.", nameof(flagType));

            ActiveFlag = (CommandFlag)Activator.CreateInstance(flagType)!;

            return ActiveFlag;
        }

        /// <summary>
        /// Clears the currently active <see cref="CommandFlag"/>. (Sets <see cref="ActiveFlag"/> to <see langword="null"/>.)
        /// </summary>
        public void ClearFlag()
        {
            ActiveFlag = null;
        }

        /// <summary>
        /// Resets the currently active <see cref="CommandFlag"/>.
        /// 
        /// <code>SetFlag(ActiveFlag.GetType());</code>
        /// </summary>
        public void ResetFlag()
        {
            SetFlag(ActiveFlag.GetType());
        }
    }
}