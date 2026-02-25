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
            if (context.Command is KernelCommandInfo cmd)
            {
                if (cmd.RequiredFlag != _commandManager.Value.ActiveFlag.GetType())
                {
                    throw new Exception("no.");
                }
            }
        }
    }
}
