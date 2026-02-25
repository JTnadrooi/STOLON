using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class KernelCommandHook : ActionHook
    {
        private readonly Lazy<CommandManager> _commandManager;

        public KernelCommandHook(Lazy<CommandManager> commandManager) : base("kernel-general-validate")
        {
            _commandManager = commandManager;
        }

        public override void PreCommand(CommandContext context)
        {
            if (context.Command is KernelCommandInfo cmd) // not all commands will be the KernelCommandInfo type. (e.g; automatically added ones)
            {
                if (cmd.RequiredFlag != _commandManager.Value.ActiveFlag.GetType())
                {
                    context.Flags |= ExecutingContextFlags.PreventCommand;
                    throw new CommandException("Required flag for this command is not active.");
                }
            }
        }
    }
}
